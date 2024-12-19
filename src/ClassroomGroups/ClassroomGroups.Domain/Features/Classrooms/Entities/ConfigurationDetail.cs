using ClassroomGroups.Domain.Features.Classrooms.Extensions;

namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public record GroupStudentsResult(
  List<StudentGroup> UpdatedStudentGroups,
  List<Group> CreatedGroups
);

public class ConfigurationDetail(
  Guid Id,
  Guid ClassroomId,
  Guid DefaultGroupId,
  string Label,
  string Description,
  List<GroupDetail> GroupDetails,
  List<ColumnDetail> ColumnDetails
)
{
  public Guid Id { get; private set; } = Id;

  public Guid ClassroomId { get; private set; } = ClassroomId;

  public Guid DefaultGroupId { get; private set; } = DefaultGroupId;

  public string Label { get; private set; } = Label;

  public string Description { get; private set; } = Description;

  public List<GroupDetail> GroupDetails { get; private set; } = GroupDetails;

  public List<ColumnDetail> ColumnDetails { get; private set; } = ColumnDetails;

  public GroupStudentsResult GroupStudents(
    IEnumerable<Field> fields,
    StudentGroupingStrategy strategy,
    int? numberOfGroups = null,
    int? studentsPerGroup = null
  )
  {
    var allStudents = GroupDetails.SelectMany(g => g.StudentDetails);

    if (numberOfGroups >= allStudents.Count() || studentsPerGroup >= allStudents.Count())
    {
      throw new Exception("You must specify a group count less than the number of students");
    }

    if (numberOfGroups <= 0 || studentsPerGroup <= 0)
    {
      throw new Exception(
        "Students per group or numbers of groups must be greater than or equal to zero."
      );
    }

    if (numberOfGroups is not null && studentsPerGroup is not null)
    {
      throw new Exception(
        "Unable to group students by number of students per group and number of groups simultaneously."
      );
    }

    if (numberOfGroups is null && studentsPerGroup is null)
    {
      throw new Exception(
        "You must group students by the number of groups or by the number of students per group."
      );
    }

    var candidateStudentDetails = GroupDetails
      .Where(g => !g.IsLocked)
      .SelectMany(g => g.StudentDetails)
      .OrderByAverage(fields);
    var existingCandidateGroups = GroupDetails
      .Where(g => !g.IsLocked && g.Id != DefaultGroupId)
      .Select(g => g.ToGroup())
      .OrderBy(g => g.Ordinal);

    var numAffectedCandidateGroups = 0;

    if (numberOfGroups is not null)
    {
      numAffectedCandidateGroups = (int)numberOfGroups;
    }
    else if (studentsPerGroup is not null)
    {
      // Why aren't candidate student details populating correctly?
      // 2 students per group mixed with default group only - numAffectedCandidateGroups should be 2! existing should be 0.
      numAffectedCandidateGroups = (int)
        Math.Ceiling((decimal)(candidateStudentDetails.Count() / studentsPerGroup));
    }

    var candidateGroups = new List<Group>(existingCandidateGroups);
    var createdGroups = new List<Group>();

    // Add groups if necessary
    var numGroupsToCreate = Math.Max(numAffectedCandidateGroups - candidateGroups.Count(), 0);
    for (var i = 0; i < numGroupsToCreate; i++)
    {
      var ordinal = GroupDetails.Count - 1 + i;
      var newGroup = new Group(Guid.NewGuid(), Id, $"Group {ordinal}", ordinal, false);
      candidateGroups.Add(newGroup);
      createdGroups.Add(newGroup);
    }

    // Remove groups if necessary
    var numCandidateGroupsToShed = candidateGroups.Count - numAffectedCandidateGroups;
    if (numCandidateGroupsToShed > 0)
    {
      candidateGroups = candidateGroups.Take(numCandidateGroupsToShed).ToList();
    }

    // Generate accurate StudentGroups
    List<StudentGroup> updatedStudentGroups = [];
    var studentPartsPlaced = 0;

    if (candidateGroups.Any())
    {
      for (var i = 0; i < candidateStudentDetails.Count(); i++)
      {
        var studentPartPosition = i % candidateGroups.Count;
        if (i != 0 && studentPartPosition == 0)
        {
          studentPartsPlaced++;
        }
        var studentDetail = candidateStudentDetails.ElementAt(i);

        var groupIndex =
          strategy == StudentGroupingStrategy.SimilarAbilities
            ? studentPartsPlaced
            : studentPartPosition;

        var ordinal =
          strategy == StudentGroupingStrategy.SimilarAbilities
            ? studentPartPosition
            : studentPartsPlaced;

        var studentGroup = new StudentGroup(
          studentDetail.StudentGroupId,
          studentDetail.Id,
          candidateGroups[groupIndex].Id,
          ordinal
        );

        updatedStudentGroups.Add(studentGroup);
      }
    }

    return new GroupStudentsResult(updatedStudentGroups, createdGroups);
  }
}
