using System.Diagnostics;
using ClassroomGroups.Algorithms;

namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public record GroupStudentsResultDetails(
  List<StudentGroup> StudentGroupsToCreate,
  List<Group> GroupsToCreate,
  List<Guid> StudentGroupIdsToDelete,
  List<Guid> GroupIdsToDelete
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
    var students = GroupDetails.Where(g => !g.IsLocked).SelectMany(g => g.StudentDetails).ToList();

    var (groupCount, errorResult) = CalculateGroupCount(
      numberOfGroups,
      studentsPerGroup,
      students.Count
    );

    if (errorResult is not null)
    {
      return errorResult;
    }

    var gIdsToDelete = GroupDetails
      .Where(g => !g.IsLocked && g.Id != DefaultGroupId)
      .Select(g => g.Id)
      .ToList();

    var sgIdsToDelete = students.Select(s => s.StudentGroupId).ToList();

    if (groupCount > 0)
    {
      var result = AssignStudentsToNewGroups(students, groupCount, strategy, fields);

      return new GroupStudentsResult(
        new GroupStudentsResultDetails(
          result.sgsToCreate,
          result.gsToCreate,
          sgIdsToDelete,
          gIdsToDelete
        ),
        null
      );
    }

    var sgsToCreate = AssignStudentsToDefaultGroup(students);
    return new GroupStudentsResult(
      new GroupStudentsResultDetails(sgsToCreate, [], sgIdsToDelete, gIdsToDelete),
      null
    );
  }

  public (List<StudentGroup> sgsToCreate, List<Group> gsToCreate) AssignStudentsToNewGroups(
    List<StudentDetail> students,
    int groupCount,
    StudentGroupingStrategy strategy,
    IEnumerable<Field> fields
  )
  {
    List<Group> gsToCreate = GetNewGroups(groupCount);

    var studentsWithScores = students.Select(s => (s, s.CalculateAverage(fields))).ToList();

    var groupsOfStudents =
      strategy == StudentGroupingStrategy.MixedAbilities
        ? studentsWithScores.PartitionIntoBalancedGroups(gsToCreate.Count)
        : studentsWithScores.PartitionIntoSimilarGroups(gsToCreate.Count);

    var sgsToCreate = gsToCreate
      .SelectMany(
        (group, i) =>
          groupsOfStudents[i]
            .OrderByDescending(s => s.CalculateAverage(fields))
            .Select((student, i) => new StudentGroup(Guid.NewGuid(), student.Id, group.Id, i + 1))
      )
      .ToList();

    return (sgsToCreate, gsToCreate);
  }

  private List<Group> GetNewGroups(int groupCount)
  {
    List<Group> groups = [];
    for (var i = 0; i < groupCount; i++)
    {
      var ordinal = i + 1;
      groups.Add(new Group(Guid.NewGuid(), Id, $"Group {ordinal}", ordinal, false));
    }
    return groups;
  }

  public List<StudentGroup> AssignStudentsToDefaultGroup(List<StudentDetail> students)
  {
    var sgsToCreate = students
      .Select(s => new StudentGroup(Guid.NewGuid(), s.Id, DefaultGroupId, s.Ordinal))
      .ToList();
    return sgsToCreate;
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
}
