namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class ConfigurationDetail(
  Guid Id,
  Guid ClassroomId,
  string Label,
  string? Description,
  List<GroupDetail> GroupDetails,
  List<ColumnDetail> ColumnDetails
)
{
  public Guid Id { get; private set; } = Id;

  public Guid ClassroomId { get; private set; } = ClassroomId;

  public string Label { get; private set; } = Label;

  public string? Description { get; private set; } = Description;

  public List<GroupDetail> GroupDetails { get; private set; } = GroupDetails;

  public List<ColumnDetail> ColumnDetails { get; private set; } = ColumnDetails;
}
