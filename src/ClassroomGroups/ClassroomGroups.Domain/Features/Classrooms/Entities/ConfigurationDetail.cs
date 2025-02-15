using System.Diagnostics;
using ClassroomGroups.Domain.Features.Classrooms.Extensions;

namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public record GroupStudentsResultDetails(
  List<StudentGroup> StudentGroupsToCreate,
  List<Group> GroupsToCreate,
  List<Guid> StudentGroupIdsToDelete,
  List<Guid> UnpopulatedGroupIds
);

public record GroupStudentsResult(
  GroupStudentsResultDetails ResultDetails,
  string? ErrorMessage = null
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
  private static readonly GroupStudentsResultDetails EMPTY_GROUP_STUDENT_RESULT_DETAILS =
    new([], [], [], []);

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
    var rankedCandidates = GroupDetails
      .Where(g => !g.IsLocked)
      .SelectMany(g => g.StudentDetails)
      .OrderByAverage(fields);

    var (groupCount, errorResult) = CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      rankedCandidates.Count()
    );
    if (errorResult is not null)
    {
      return errorResult;
    }

    var oldGroups = GroupDetails
      .Where(g => !g.IsLocked && g.Id != DefaultGroupId)
      .Select(g => g.ToGroup())
      .OrderBy(g => g.Ordinal);

    if (groupCount <= 0)
    {
      var defaultGroups = rankedCandidates
        .Select(s => new StudentGroup(Guid.NewGuid(), s.Id, DefaultGroupId, s.Ordinal))
        .ToList();
      var sgIdsToDelete = rankedCandidates.Select(s => s.StudentGroupId).ToList();
      var unpopulatedGroupIds = oldGroups.Select(g => g.Id).ToList();
      return new GroupStudentsResult(
        new GroupStudentsResultDetails(defaultGroups, [], sgIdsToDelete, unpopulatedGroupIds)
      );
    }

    var newGroups = new List<Group>(oldGroups);
    var createdGroups = new List<Group>();

    var numGroupsToCreate = Math.Max(groupCount - newGroups.Count, 0);
    for (var i = 0; i < numGroupsToCreate; i++)
    {
      var ordinal = GroupDetails.Count - 1 + i;
      var newGroup = new Group(Guid.NewGuid(), Id, $"Group {ordinal + 1}", ordinal, false);
      newGroups.Add(newGroup);
      createdGroups.Add(newGroup);
    }
    var numNewGroupsToDisregard = newGroups.Count - groupCount;
    if (numNewGroupsToDisregard > 0)
    {
      newGroups = newGroups.Take(groupCount).ToList();
    }
    var usedGroupIds = newGroups.Select(g => g.Id);
    var unusedGroupIds = oldGroups.Select(g => g.Id).Except(usedGroupIds).ToList();

    var (studentGroupsToCreate, studentGroupIdsToDelete) =
      strategy == StudentGroupingStrategy.MixedAbilities
        ? GenerateMixedAbilityStudentGroups(newGroups, rankedCandidates)
        : GenerateSimilarAbilityStudentGroups(newGroups, rankedCandidates);

    return new GroupStudentsResult(
      new GroupStudentsResultDetails(
        studentGroupsToCreate,
        createdGroups,
        studentGroupIdsToDelete,
        unusedGroupIds
      )
    );
  }

  public (int groupCount, GroupStudentsResult? errorResult) CalculateGroupCount(
    int? numberOfGroups,
    int? studentsPerGroup,
    int numCandidateStudents
  )
  {
    static GroupStudentsResult? CreateErrorResult(string message) =>
      new(EMPTY_GROUP_STUDENT_RESULT_DETAILS, message);

    if (numberOfGroups is not null && studentsPerGroup is not null)
      return (-1, CreateErrorResult("Cannot specify multiple group counts"));

    if (numberOfGroups is null && studentsPerGroup is null)
      return (-1, CreateErrorResult("Group by either number of groups or students per group"));

    if (numberOfGroups < 0 || studentsPerGroup < 0)
      return (-1, CreateErrorResult("Group count must be a positive number"));

    if (numberOfGroups >= numCandidateStudents || studentsPerGroup >= numCandidateStudents)
      return (-1, CreateErrorResult("Group count must be less than the available student count"));

    if (numberOfGroups == 0 || studentsPerGroup == 0)
      return (0, null);

    if (numberOfGroups is not null)
      return ((int)numberOfGroups, null);

    if (studentsPerGroup is not null)
    {
      var exactStudentsPerGroup = (decimal)numCandidateStudents / studentsPerGroup;
      var groupCount = (int)Math.Ceiling((decimal)exactStudentsPerGroup);
      return (groupCount, null);
    }
    throw new UnreachableException();
  }

  private static (List<StudentGroup>, List<Guid>) GenerateMixedAbilityStudentGroups(
    IEnumerable<Group> groups,
    IEnumerable<StudentDetail> rankedStudentDetails
  )
  {
    List<StudentGroup> studentGroupsToCreate = [];
    List<Guid> studentGroupIdsToDelete = [];

    if (!groups.Any())
    {
      return (studentGroupsToCreate, studentGroupIdsToDelete);
    }

    var ordinal = 0;

    for (var studentIndex = 0; studentIndex < rankedStudentDetails.Count(); studentIndex++)
    {
      var groupIndex = studentIndex % groups.Count();
      if (studentIndex != 0 && groupIndex == 0)
      {
        ordinal++;
      }
      var studentDetail = rankedStudentDetails.ElementAt(studentIndex);

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

  private static (List<StudentGroup>, List<Guid>) GenerateSimilarAbilityStudentGroups(
    IEnumerable<Group> groups,
    IEnumerable<StudentDetail> rankedStudentDetails
  )
  {
    List<StudentGroup> studentGroupsToCreate = [];
    List<Guid> studentGroupIdsToDelete = [];

    if (!groups.Any())
    {
      return (studentGroupsToCreate, studentGroupIdsToDelete);
    }

    var maxNumberStudentsPerGroup = (int)
      Math.Ceiling((decimal)rankedStudentDetails.Count() / groups.Count());

    var groupIndex = 0;

    for (var studentIndex = 0; studentIndex < rankedStudentDetails.Count(); studentIndex++)
    {
      var ordinal = studentIndex % maxNumberStudentsPerGroup;

      if (studentIndex != 0 && ordinal == 0)
      {
        groupIndex++;
      }
      var studentDetail = rankedStudentDetails.ElementAt(studentIndex);

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
