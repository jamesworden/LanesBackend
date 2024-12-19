using ClassroomGroups.Domain.Features.Classrooms.Extensions;

namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public record GroupStudentsResult(
  List<StudentGroup> StudentGroupsToCreate,
  List<Group> GroupsToCreate,
  List<Guid> StudentGroupIdsToDelete,
  List<Guid> UnpopulatedGroupIds
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

    var oldGroups = GroupDetails
      .Where(g => !g.IsLocked && g.Id != DefaultGroupId)
      .Select(g => g.ToGroup())
      .OrderBy(g => g.Ordinal);

    if (numberOfGroups <= 0 || studentsPerGroup <= 0)
    {
      var defaultStudentGroups = candidateStudentDetails
        .Select(s => new StudentGroup(Guid.NewGuid(), s.Id, DefaultGroupId, s.Ordinal))
        .ToList();
      var sgIdsToDelete = candidateStudentDetails.Select(s => s.StudentGroupId).ToList();
      var unpopulatedGroupIds = oldGroups.Select(g => g.Id).ToList();
      return new GroupStudentsResult(defaultStudentGroups, [], sgIdsToDelete, unpopulatedGroupIds);
    }

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
    var newGroups = new List<Group>(oldGroups);
    var createdGroups = new List<Group>();

    var numGroupsToCreate = Math.Max(numAffectedCandidateGroups - newGroups.Count(), 0);
    for (var i = 0; i < numGroupsToCreate; i++)
    {
      var ordinal = GroupDetails.Count - 1 + i;
      var newGroup = new Group(Guid.NewGuid(), Id, $"Group {ordinal}", ordinal, false);
      newGroups.Add(newGroup);
      createdGroups.Add(newGroup);
    }
    var numNewGroupsToDisregard = newGroups.Count - numAffectedCandidateGroups;
    if (numNewGroupsToDisregard > 0)
    {
      newGroups = newGroups.Take(numNewGroupsToDisregard).ToList();
    }
    var usedGroupIds = newGroups.Select(g => g.Id);
    var unusedGroupIds = oldGroups.Select(g => g.Id).Except(usedGroupIds).ToList();
    var (studentGroupsToCreate, studentGroupIdsToDelete) = GenerateNewStudentGroups(
      newGroups,
      candidateStudentDetails,
      strategy
    );

    return new GroupStudentsResult(
      studentGroupsToCreate,
      createdGroups,
      studentGroupIdsToDelete,
      unusedGroupIds
    );
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
