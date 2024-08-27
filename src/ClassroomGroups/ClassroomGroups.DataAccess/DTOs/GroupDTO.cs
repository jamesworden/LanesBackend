using System.ComponentModel.DataAnnotations;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class GroupDTO
{
  [Key]
  public int Key { get; set; }

  public Guid Id { get; private set; }

  public ConfigurationDTO ConfigurationDTO { get; set; } = null!;
  public int ConfigurationKey { get; set; }
  public Guid ConfigurationId { get; private set; }

  public string Label { get; private set; } = "";

  public int Ordinal { get; private set; }

  public Group ToGroup()
  {
    return new Group(Id, ConfigurationId, Label, Ordinal);
  }
}
