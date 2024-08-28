using System.ComponentModel.DataAnnotations;

namespace ClassroomGroups.DataAccess.DTOs;

public class ClassroomDTO
{
  [Key]
  public int Key { get; set; }
  public Guid Id { get; private set; }
  public string Label { get; private set; } = "";
  public string? Description { get; private set; } = "";

  public AccountDTO AccountDTO { get; private set; } = null!;
  public int AccountKey { get; set; }

  public ICollection<StudentDTO> Students { get; } = [];

  public ICollection<FieldDTO> Fields { get; } = [];

  public ICollection<ConfigurationDTO> Configurations { get; } = [];
}
