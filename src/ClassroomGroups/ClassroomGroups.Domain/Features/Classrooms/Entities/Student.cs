namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class Student(Guid Id, Guid ClassroomId)
{
  public Guid Id { get; private set; } = Id;

  public Guid ClassroomId { get; private set; } = ClassroomId;
}
