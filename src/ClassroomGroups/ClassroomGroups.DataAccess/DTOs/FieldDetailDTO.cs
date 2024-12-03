using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class FieldDetailDTO(Guid Id, Guid ClassroomId, string Label, FieldType Type)
{
  public Guid Id = Id;

  public Guid AccountId = ClassroomId;

  public string Label = Label;

  public FieldType Type = Type;

  public FieldDetail ToFieldDetail()
  {
    return new FieldDetail(Id, AccountId, Label, Type);
  }
}
