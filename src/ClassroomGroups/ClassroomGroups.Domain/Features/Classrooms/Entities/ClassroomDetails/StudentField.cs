namespace ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

public class StudentField(Guid Id, Guid StudentId, Guid FieldId, string Value)
{
  public Guid Id { get; private set; } = Id;

  public Guid StudentId { get; private set; } = StudentId;

  public Guid FieldId { get; private set; } = FieldId;

  public string Value { get; private set; } = Value;
}
