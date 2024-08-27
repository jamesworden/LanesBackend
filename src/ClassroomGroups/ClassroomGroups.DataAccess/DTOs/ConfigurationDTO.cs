using System.ComponentModel.DataAnnotations;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class ConfigurationDTO
{
  [Key]
  public int Key { get; set; }

  public Guid Id { get; private set; }

  public ClassroomDTO ClassroomDTO { get; private set; } = null!;
  public int ClassroomKey { get; private set; }
  public Guid ClassroomId { get; private set; }

  public string Label { get; private set; } = "";

  public string? Description { get; private set; } = "";

  public Configuration ToConfiguration()
  {
    return new Configuration(Id, ClassroomId, Label, Description);
  }
}
