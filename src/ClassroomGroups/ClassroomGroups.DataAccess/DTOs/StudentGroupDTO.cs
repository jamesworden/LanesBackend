using System.ComponentModel.DataAnnotations;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentGroupDTO
{
  [Key]
  public int Key { get; set; }
  public int Ordinal { get; private set; }
  public Guid Id { get; private set; }

  public StudentDTO StudentDTO = null!;
  public int StudentKey { get; private set; }

  public GroupDTO GroupDTO = null!;
  public int GroupKey { get; private set; }
}
