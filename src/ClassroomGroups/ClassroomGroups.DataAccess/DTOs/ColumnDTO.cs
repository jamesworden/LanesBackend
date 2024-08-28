using System.ComponentModel.DataAnnotations;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class ColumnDTO
{
  [Key]
  public int Key { get; set; }

  public Guid Id { get; private set; }

  public ConfigurationDTO ConfigurationDTO { get; set; } = null!;
  public int ConfigurationKey { get; set; }

  public FieldDTO FieldDTO { get; private set; } = null!;
  public int FieldKey { get; set; }

  public int Ordinal { get; private set; }

  public bool Enabled { get; private set; } = true;

  public ColumnSort Sort { get; private set; } = ColumnSort.ASCENDING;
}
