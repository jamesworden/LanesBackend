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
  public Guid Id { get; private set; } = Id;

  public Guid ConfigurationId { get; private set; } = ConfigurationId;

  public Guid FieldId { get; private set; } = FieldId;

  public int Ordinal { get; private set; } = Ordinal;

  public ColumnSort Sort { get; private set; } = Sort;

  public bool Enabled { get; private set; } = Enabled;

  public FieldType Type { get; private set; } = Type;
}
