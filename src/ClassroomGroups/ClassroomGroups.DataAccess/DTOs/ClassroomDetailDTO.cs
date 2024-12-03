using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class ClassroomDetailDTO(Guid Id, Guid AccountId, string Label, string Description)
{
  public Guid Id = Id;

  public Guid AccountId = AccountId;

  public string Label = Label;

  public string Description = Description;

  public ClassroomDetail ToClassroomDetail(List<FieldDetail> FieldDetails)
  {
    return new ClassroomDetail(Id, AccountId, Label, Description, FieldDetails);
  }
}
