namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class StudentDetail(
  Guid Id,
  Guid GroupId,
  int Ordinal,
  Dictionary<Guid, string> FieldIdsToValues
)
{
  public Guid Id { get; private set; } = Id;

  public Guid GroupId { get; private set; } = GroupId;

  public int Ordinal { get; private set; } = Ordinal;

  public Dictionary<Guid, string> FieldIdsToValues { get; private set; } = FieldIdsToValues;
}
