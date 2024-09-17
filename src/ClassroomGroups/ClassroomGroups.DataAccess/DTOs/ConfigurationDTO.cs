using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class ConfigurationDTO
{
  [Key]
  public int Key { get; set; }

  [InverseProperty("ConfigurationId")]
  public Guid Id { get; set; }
  public string Label { get; set; } = "";
  public string Description { get; set; } = "";

  public ClassroomDTO ClassroomDTO { get; set; } = null!;
  public int ClassroomKey { get; set; }
  public Guid ClassroomId { get; set; }

  public ICollection<FieldDTO> Fields { get; set; } = [];
  public ICollection<ColumnDTO> Columns { get; set; } = [];

  public ICollection<GroupDTO> Groups { get; set; } = [];

  public Configuration ToConfiguration()
  {
    return new Configuration(Id, ClassroomId, Label, Description);
  }
}
