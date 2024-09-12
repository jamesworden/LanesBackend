using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentGroupDTO
{
  [Key]
  public int Key { get; set; }
  public int Ordinal { get; private set; }

  [InverseProperty("StudentGroupId")]
  public Guid Id { get; private set; }

  public StudentDTO StudentDTO = null!;
  public int StudentKey { get; private set; }
  public Guid StudentId { get; private set; }

  public GroupDTO GroupDTO = null!;
  public int GroupKey { get; private set; }
  public Guid GroupId { get; private set; }

  public StudentGroup ToStudentGroup()
  {
    return new StudentGroup(Id, StudentId, GroupId, Ordinal);
  }
}
