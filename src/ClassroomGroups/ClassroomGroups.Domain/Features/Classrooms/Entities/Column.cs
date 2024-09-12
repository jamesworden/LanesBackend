namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class Column(
  Guid Id,
  Guid ConfigurationId,
  Guid FieldId,
  int Ordinal,
  bool Enabled,
  ColumnSort Sort
)
{
  public Guid Id { get; private set; } = Id;

  public Guid ConfigurationId { get; private set; } = ConfigurationId;

  public Guid FieldId { get; private set; } = FieldId;

  public int Ordinal { get; private set; } = Ordinal;

  public bool Enabled { get; private set; } = Enabled;

  public ColumnSort Sort { get; private set; } = Sort;

  public ColumnDetail ToColumnDetail(FieldType Type)
  {
    return new ColumnDetail(Id, ConfigurationId, FieldId, Ordinal, Sort, Enabled, Type);
  }
}
