using System.ComponentModel.DataAnnotations;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentFieldDTO
{
  [Key]
  public int Key { get; set; }

  public Guid Id { get; private set; }

  public StudentDTO StudentDTO = null!;
  public int StudentKey { get; private set; }
  public Guid StudentId { get; private set; }

  public FieldDTO FieldDTO = null!;
  public int FieldKey { get; private set; }
  public Guid FieldId { get; private set; }

  public string Value { get; private set; } = "";

  public StudentField ToStudentField()
  {
    return new StudentField(Id, StudentId, FieldId, Value);
  }
}