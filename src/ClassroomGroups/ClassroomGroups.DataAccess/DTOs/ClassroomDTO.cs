using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class ClassroomDTO
{
  [Key]
  public int Key { get; set; }

  [InverseProperty("ClassroomId")]
  public Guid Id { get; private set; }
  public string Label { get; private set; } = "";
  public string? Description { get; private set; } = "";

  public AccountDTO AccountDTO { get; private set; } = null!;
  public int AccountKey { get; set; }
  public Guid AccountId { get; private set; }

  public ICollection<StudentDTO> Students { get; } = [];

  public ICollection<FieldDTO> Fields { get; } = [];

  public ICollection<ConfigurationDTO> Configurations { get; } = [];

  public Classroom ToClassroom()
  {
    return new Classroom(Id, AccountId, Label, Description);
  }
}
