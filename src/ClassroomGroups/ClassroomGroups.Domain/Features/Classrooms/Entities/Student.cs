namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class Student(Guid Id, Guid ClassroomId)
{
  public Guid Id { get; private set; } = Id;

  public Guid ClassroomId { get; private set; } = ClassroomId;

  public StudentWithFields WithFields(Dictionary<Guid, string> FieldIdsToValues)
  {
    return new StudentWithFields(Id, ClassroomId, FieldIdsToValues);
  }
}
