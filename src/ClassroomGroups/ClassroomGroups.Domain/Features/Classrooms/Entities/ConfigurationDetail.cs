using ClassroomGroups.Domain.Features.Classrooms.Extensions;

namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public record GroupStudentsResult(
  List<StudentGroup> StudentGroupsToCreate,
  List<Group> GroupsToCreate,
  List<Guid> StudentGroupIdsToDelete
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

    if (numberOfGroups <= 0 || studentsPerGroup <= 0)
    {
      var defaultStudentGroups = candidateStudentDetails
        .Select(s => new StudentGroup(Guid.NewGuid(), s.Id, DefaultGroupId, s.Ordinal))
        .ToList();
      var sgIdsToDelete = candidateStudentDetails.Select(s => s.StudentGroupId).ToList();
      return new GroupStudentsResult(defaultStudentGroups, [], sgIdsToDelete);
    }

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
      numAffectedCandidateGroups = (int)
        Math.Ceiling((decimal)(candidateStudentDetails.Count() / studentsPerGroup));
    }
    var candidateGroups = new List<Group>(existingCandidateGroups);
    var createdGroups = new List<Group>();

    var numGroupsToCreate = Math.Max(numAffectedCandidateGroups - candidateGroups.Count(), 0);
    for (var i = 0; i < numGroupsToCreate; i++)
    {
      var ordinal = GroupDetails.Count - 1 + i;
      var newGroup = new Group(Guid.NewGuid(), Id, $"Group {ordinal}", ordinal, false);
      candidateGroups.Add(newGroup);
      createdGroups.Add(newGroup);
    }
    var numGroupsToDelete = candidateGroups.Count - numAffectedCandidateGroups;
    if (numGroupsToDelete > 0)
    {
      candidateGroups = candidateGroups.Take(numGroupsToDelete).ToList();
    }
    var (studentGroupsToCreate, studentGroupIdsToDelete) = GenerateNewStudentGroups(
      candidateGroups,
      candidateStudentDetails,
      strategy
    );

    return new GroupStudentsResult(studentGroupsToCreate, createdGroups, studentGroupIdsToDelete);
  }

  private static (List<StudentGroup>, List<Guid>) GenerateNewStudentGroups(
    IEnumerable<Group> groups,
    IEnumerable<StudentDetail> studentDetails,
    StudentGroupingStrategy strategy
  )
  {
    List<StudentGroup> studentGroupsToCreate = [];
    List<Guid> studentGroupIdsToDelete = [];

    if (!groups.Any())
    {
      return (studentGroupsToCreate, studentGroupIdsToDelete);
    }

    var studentPartsPlaced = 0;

    for (var i = 0; i < studentDetails.Count(); i++)
    {
      var studentPartPosition = i % groups.Count();
      if (i != 0 && studentPartPosition == 0)
      {
        studentPartsPlaced++;
      }
      var studentDetail = studentDetails.ElementAt(i);

      var groupIndex =
        strategy == StudentGroupingStrategy.SimilarAbilities
          ? studentPartsPlaced
          : studentPartPosition;

      var ordinal =
        strategy == StudentGroupingStrategy.SimilarAbilities
          ? studentPartPosition
          : studentPartsPlaced;

      var studentGroup = new StudentGroup(
        Guid.NewGuid(),
        studentDetail.Id,
        groups.ElementAt(groupIndex).Id,
        ordinal
      );

      studentGroupsToCreate.Add(studentGroup);
      studentGroupIdsToDelete.Add(studentDetail.StudentGroupId);
    }

    return (studentGroupsToCreate, studentGroupIdsToDelete);
  }
}
