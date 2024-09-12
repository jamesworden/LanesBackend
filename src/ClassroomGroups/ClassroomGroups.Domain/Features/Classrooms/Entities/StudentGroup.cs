namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class StudentGroup(Guid Id, Guid StudentId, Guid GroupId, int Ordinal)
{
  public Guid Id { get; private set; } = Id;

  public Guid StudentId { get; private set; } = StudentId;

  public Guid GroupId { get; private set; } = GroupId;

  public int Ordinal { get; private set; } = Ordinal;
}
