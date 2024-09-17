namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class Classroom(Guid Id, Guid AccountId, string Label, string Description)
{
  public Guid Id { get; private set; } = Id;

  public Guid AccountId { get; private set; } = AccountId;

  public string Label { get; private set; } = Label;

  public string Description { get; private set; } = Description;

  public ClassroomDetail ToClassroomDetail(List<FieldDetail> fieldDetails)
  {
    return new ClassroomDetail(Id, AccountId, Label, Description, fieldDetails);
  }
}
