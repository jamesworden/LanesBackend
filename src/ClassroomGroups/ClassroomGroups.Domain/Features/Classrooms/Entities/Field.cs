namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class Field(Guid Id, Guid ClassroomId, string Label, FieldType Type)
{
  public Guid Id { get; private set; } = Id;

  public Guid ClassroomId { get; private set; } = ClassroomId;

  public string Label { get; private set; } = Label;

  public FieldType Type { get; private set; } = Type;

  public FieldDetail ToFieldDetail()
  {
    return new FieldDetail(Id, ClassroomId, Label, Type);
  }
}
