using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentGroupDTO
{
  [Key]
  public int Key { get; set; }
  public int Ordinal { get; set; }

  [InverseProperty("StudentGroupId")]
  public Guid Id { get; set; }

  public StudentDTO StudentDTO = null!;
  public int StudentKey { get; set; }
  public Guid StudentId { get; set; }

  public GroupDTO GroupDTO = null!;
  public int GroupKey { get; set; }
  public Guid GroupId { get; set; }

  public StudentGroup ToStudentGroup()
  {
    return new StudentGroup(Id, StudentId, GroupId, Ordinal);
  }
}
