using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class ColumnDTO
{
  [Key]
  public int Key { get; set; }

  [InverseProperty("ColumnId")]
  public Guid Id { get; set; }

  public ConfigurationDTO ConfigurationDTO { get; set; } = null!;
  public int ConfigurationKey { get; set; }
  public Guid ConfigurationId { get; set; }

  public FieldDTO FieldDTO { get; set; } = null!;
  public int FieldKey { get; set; }
  public Guid FieldId { get; set; }

  public int Ordinal { get; set; }

  public bool Enabled { get; set; } = true;

  public ColumnSort Sort { get; set; } = ColumnSort.ASCENDING;

  public Column ToColumn()
  {
    return new Column(Id, ConfigurationId, FieldId, Ordinal, Enabled, Sort);
  }
}
