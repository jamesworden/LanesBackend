using System.Reflection.Emit;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class ColumnDetailDTO(
  Guid Id,
  Guid ConfigurationId,
  Guid FieldId,
  int Ordinal,
  ColumnSort Sort,
  bool Enabled,
  FieldType Type,
  string Label
)
{
  public Guid Id = Id;

  public Guid ConfigurationId = ConfigurationId;

  public Guid FieldId = FieldId;

  public int Ordinal = Ordinal;

  public ColumnSort Sort = Sort;

  public bool Enabled = Enabled;

  public FieldType Type = Type;

  public string Label = Label;

  public ColumnDetail ToColumnDetail()
  {
    return new ColumnDetail(Id, ConfigurationId, FieldId, Ordinal, Sort, Enabled, Type, Label);
  }
}
