using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentDetailDTO(
  Guid Id,
  Guid GroupId,
  int StudentGroupOrdinal,
  Guid StudentGroupId,
  Dictionary<Guid, string> FieldIdsToValues
)
{
  public Guid Id = Id;

  public Guid GroupId = GroupId;

  public int StudentGroupOrdinal = StudentGroupOrdinal;

  public Guid StudentGroupId = StudentGroupId;

  public Dictionary<Guid, string> FieldIdsToValues { get; private set; } = FieldIdsToValues;

  public StudentDetail ToStudentDetail()
  {
    return new StudentDetail(Id, GroupId, StudentGroupOrdinal, StudentGroupId, FieldIdsToValues);
  }
}
