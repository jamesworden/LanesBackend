namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class StudentWithFields(Guid Id, Guid ClassroomId, Dictionary<Guid, string> FieldIdsToValues)
{
  public Guid Id { get; private set; } = Id;

  public Guid ClassroomId { get; private set; } = ClassroomId;

  public Dictionary<Guid, string> FieldIdsToValues { get; private set; } = FieldIdsToValues;
}
