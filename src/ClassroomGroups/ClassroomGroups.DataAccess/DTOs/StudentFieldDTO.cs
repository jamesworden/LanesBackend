using System.ComponentModel.DataAnnotations;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentFieldDTO
{
  [Key]
  public int Key { get; set; }
  public string Value { get; private set; } = "";
  public Guid Id { get; private set; }

  public StudentDTO StudentDTO = null!;
  public int StudentKey { get; private set; }

  public FieldDTO FieldDTO = null!;
  public int FieldKey { get; private set; }
}
