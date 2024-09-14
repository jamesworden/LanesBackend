using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class FieldDTO
{
  [Key]
  public int Key { get; private set; }

  [InverseProperty("FieldId")]
  public Guid Id { get; set; }
  public FieldType Type { get; set; } = FieldType.TEXT;
  public string Label { get; set; } = "";

  public ClassroomDTO ClassroomDTO { get; private set; } = null!;
  public int ClassroomKey { get; set; }
  public Guid ClassroomId { get; set; }

  public ICollection<StudentDTO> Students { get; private set; } = [];
  public ICollection<StudentFieldDTO> StudentFields { get; private set; } = [];

  public ICollection<ConfigurationDTO> Configurations { get; private set; } = [];
  public ICollection<ColumnDTO> Columns { get; private set; } = [];

  public Field ToField()
  {
    return new Field(Id, ClassroomId, Label, Type);
  }
}
