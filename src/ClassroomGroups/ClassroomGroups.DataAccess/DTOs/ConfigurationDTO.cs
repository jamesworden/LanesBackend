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

  // Because there's a one to one relationship between configurations and their default groups,
  // we must be able to create one entity before the other. For this reason, the default
  // group is temporarily null so that we can create a configuration before any group exists.
  public GroupDTO? DefaultGroupDTO { get; set; } = null!;
  public int? DefaultGroupKey { get; set; }
  public Guid? DefaultGroupId { get; set; }

  public Configuration ToConfiguration()
  {
    return new Configuration(Id, ClassroomId, DefaultGroupId ?? Guid.Empty, Label, Description);
  }
}
