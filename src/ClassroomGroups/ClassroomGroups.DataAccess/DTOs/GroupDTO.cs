using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class GroupDTO
{
  [Key]
  public int Key { get; set; }

  [InverseProperty("GroupId")]
  public Guid Id { get; set; }
  public string Label { get; set; } = "";
  public int Ordinal { get; set; }

  public ConfigurationDTO ConfigurationDTO { get; set; } = null!;
  public int ConfigurationKey { get; set; }
  public Guid ConfigurationId { get; set; }

  public ICollection<StudentDTO> Students { get; } = [];
  public ICollection<StudentGroupDTO> StudentGroups { get; set; } = [];

  public Group ToGroup()
  {
    return new Group(Id, ConfigurationId, Label, Ordinal);
  }
}
