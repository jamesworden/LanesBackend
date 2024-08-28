using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class FieldDTO
{
  [Key]
  public int Key { get; set; }

  [InverseProperty("FieldId")]
  public Guid Id { get; private set; }
  public FieldType Type { get; private set; } = FieldType.TEXT;
  public string Label { get; private set; } = "";

  public ClassroomDTO ClassroomDTO { get; private set; } = null!;
  public int ClassroomKey { get; private set; }
  public Guid ClassroomId { get; private set; }

  public ICollection<StudentDTO> Students { get; } = [];
  public ICollection<StudentFieldDTO> StudentFields { get; set; } = [];

  public ICollection<ConfigurationDTO> Configurations { get; } = [];
  public ICollection<ColumnDTO> Columns { get; set; } = [];
}
