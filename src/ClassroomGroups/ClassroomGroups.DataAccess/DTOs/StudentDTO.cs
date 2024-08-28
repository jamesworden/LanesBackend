using System.ComponentModel.DataAnnotations;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentDTO
{
  [Key]
  public int Key { get; set; }
  public Guid Id { get; private set; }

  public ClassroomDTO ClassroomDTO { get; private set; } = null!;
  public int ClassroomKey { get; set; }

  public ICollection<FieldDTO> Fields { get; } = [];
  public ICollection<StudentFieldDTO> StudentFields { get; set; } = [];

  public ICollection<GroupDTO> Groups { get; } = [];
  public ICollection<StudentGroupDTO> StudentGroups { get; set; } = [];
}
