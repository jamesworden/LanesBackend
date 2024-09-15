using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentFieldDTO
{
  [Key]
  public int Key { get; set; }
  public string Value { get; set; } = "";

  [InverseProperty("StudentFieldId")]
  public Guid Id { get; set; }

  public StudentDTO StudentDTO = null!;
  public int StudentKey { get; set; }
  public Guid StudentId { get; set; }

  public FieldDTO FieldDTO = null!;
  public int FieldKey { get; set; }
  public Guid FieldId { get; set; }

  public StudentField ToStudentField()
  {
    return new StudentField(Id, StudentId, FieldId, Value);
  }
}
