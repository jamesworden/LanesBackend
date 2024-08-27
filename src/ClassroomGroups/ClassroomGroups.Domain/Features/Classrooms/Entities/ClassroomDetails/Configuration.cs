namespace ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

public class Configuration(Guid Id, Guid ClassroomId, string Label, string? Description)
{
  public Guid Id { get; private set; } = Id;

  public Guid ClassroomId { get; private set; } = ClassroomId;

  public string Label { get; private set; } = Label;

  public string? Description { get; private set; } = Description;
}
