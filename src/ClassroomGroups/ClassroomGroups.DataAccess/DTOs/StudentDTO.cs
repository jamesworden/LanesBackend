using System.ComponentModel.DataAnnotations;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentDTO
{
  [Key]
  public int Key { get; set; }

  public Guid Id { get; private set; }

  public ClassroomDTO ClassroomDTO { get; private set; } = null!;
  public int ClassroomKey { get; set; }
  public Guid ClassroomId { get; private set; }

  public Student ToStudent()
  {
    return new Student(Id, ClassroomId);
  }
}
