namespace ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

public class Group(Guid Id, Guid ConfigurationId, string Label, int Ordinal)
{
  public Guid Id { get; private set; } = Id;

  public Guid ConfigurationId { get; private set; } = ConfigurationId;

  public string Label { get; private set; } = Label;

  public int Ordinal { get; private set; } = Ordinal;
}
