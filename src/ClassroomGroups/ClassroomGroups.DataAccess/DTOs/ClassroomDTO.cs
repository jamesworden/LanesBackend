using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class ClassroomDTO
{
  [Key]
  public int Key { get; set; }

  [InverseProperty("ClassroomId")]
  public Guid Id { get; set; }
  public string Label { get; set; } = "";
  public string Description { get; set; } = "";

  public AccountDTO AccountDTO { get; set; } = null!;
  public int AccountKey { get; set; }
  public Guid AccountId { get; set; }

  public ICollection<StudentDTO> Students { get; set; } = [];

  public ICollection<FieldDTO> Fields { get; set; } = [];

  public ICollection<ConfigurationDTO> Configurations { get; set; } = [];

  public Classroom ToClassroom()
  {
    return new Classroom(Id, AccountId, Label, Description);
  }
}
