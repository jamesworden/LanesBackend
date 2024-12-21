namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class Group(Guid Id, Guid ConfigurationId, string Label, int Ordinal, bool IsLocked)
{
  public Guid Id { get; private set; } = Id;

  public Guid ConfigurationId { get; private set; } = ConfigurationId;

  public string Label { get; private set; } = Label;

  public int Ordinal { get; private set; } = Ordinal;

  public bool IsLocked { get; private set; } = IsLocked;
}
