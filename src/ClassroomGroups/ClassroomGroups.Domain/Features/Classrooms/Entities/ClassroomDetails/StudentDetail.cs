namespace ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

public class StudentDetail(Guid Id, Guid GroupId, int Ordinal)
{
  public Guid Id { get; private set; } = Id;

  public Guid GroupId { get; private set; } = GroupId;

  public int Ordinal { get; private set; } = Ordinal;
}
