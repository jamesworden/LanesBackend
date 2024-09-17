namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class ClassroomDetail(
  Guid Id,
  Guid AccountId,
  string Label,
  string Description,
  List<FieldDetail> FieldDetails
)
{
  public Guid Id { get; private set; } = Id;

  public Guid AccountId { get; private set; } = AccountId;

  public string Label { get; private set; } = Label;

  public string Description { get; private set; } = Description;

  public List<FieldDetail> FieldDetails { get; private set; } = FieldDetails;
}
