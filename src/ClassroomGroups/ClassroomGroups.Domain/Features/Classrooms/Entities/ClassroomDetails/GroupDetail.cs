namespace ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

public class GroupDetail(
  Guid Id,
  Guid ConfigurationId,
  string Label,
  int Ordinal,
  List<StudentDetail> StudentDetails
)
{
  public Guid Id { get; private set; } = Id;

  public Guid ConfigurationId { get; private set; } = ConfigurationId;

  public string Label { get; private set; } = Label;

  public int Ordinal { get; private set; } = Ordinal;

  public List<StudentDetail> StudentDetails { get; private set; } = StudentDetails;
}
