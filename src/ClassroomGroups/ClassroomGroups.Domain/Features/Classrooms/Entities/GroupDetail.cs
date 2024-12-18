namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class GroupDetail(
  Guid Id,
  Guid ConfigurationId,
  string Label,
  int Ordinal,
  List<StudentDetail> StudentDetails,
  bool IsLocked
)
{
  public Guid Id { get; private set; } = Id;

  public Guid ConfigurationId { get; private set; } = ConfigurationId;

  public string Label { get; private set; } = Label;

  public int Ordinal { get; private set; } = Ordinal;

  public List<StudentDetail> StudentDetails { get; private set; } = StudentDetails;

  public bool IsLocked { get; private set; } = IsLocked;
}
