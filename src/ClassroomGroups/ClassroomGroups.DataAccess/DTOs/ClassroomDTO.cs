using System.ComponentModel.DataAnnotations;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class ClassroomDTO
{
  [Key]
  public int Key { get; set; }

  public Guid Id { get; private set; }

  public AccountDTO AccountDTO { get; private set; } = null!;
  public int AccountKey { get; set; }
  public Guid AccountId { get; private set; }

  public string Label { get; private set; } = "";

  public string? Description { get; private set; } = "";

  public Classroom ToClassroom()
  {
    return new Classroom(Id, AccountId, Label, Description);
  }
}
