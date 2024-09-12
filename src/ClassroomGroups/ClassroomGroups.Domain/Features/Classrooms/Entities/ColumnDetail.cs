namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class ColumnDetail(
  Guid Id,
  Guid ConfigurationId,
  Guid FieldId,
  int Ordinal,
  ColumnSort Sort,
  bool Enabled,
  FieldType Type
)
{
  public Guid Id = Id;

  public Guid ConfigurationId = ConfigurationId;

  public Guid FieldId = FieldId;

  public int Ordinal = Ordinal;

  public ColumnSort Sort = Sort;

  public bool Enabled = Enabled;

  public FieldType Type = Type;
}
