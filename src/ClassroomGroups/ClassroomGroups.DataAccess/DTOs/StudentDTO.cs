using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentDTO
{
  [Key]
  public int Key { get; set; }

  [InverseProperty("StudentId")]
  public Guid Id { get; private set; }

  public ClassroomDTO ClassroomDTO { get; private set; } = null!;
  public int ClassroomKey { get; set; }
  public Guid ClassroomId { get; private set; }

  public ICollection<FieldDTO> Fields { get; } = [];
  public ICollection<StudentFieldDTO> StudentFields { get; set; } = [];

  public ICollection<GroupDTO> Groups { get; } = [];
  public ICollection<StudentGroupDTO> StudentGroups { get; set; } = [];

  public Student ToStudent()
  {
    return new Student(Id, ClassroomId);
  }
}
