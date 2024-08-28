using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClassroomGroups.DataAccess.DTOs;

public class GroupDTO
{
  [Key]
  public int Key { get; set; }

  [InverseProperty("GroupId")]
  public Guid Id { get; private set; }
  public string Label { get; private set; } = "";
  public int Ordinal { get; private set; }

  public ConfigurationDTO ConfigurationDTO { get; set; } = null!;
  public int ConfigurationKey { get; set; }
  public Guid ConfigurationId { get; private set; }

  public ICollection<StudentDTO> Students { get; } = [];
  public ICollection<StudentGroupDTO> StudentGroups { get; set; } = [];
}
