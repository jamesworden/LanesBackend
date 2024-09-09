using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class StudentDetailDTO(Guid Id, Guid GroupId, int StudentGroupOrdinal)
{
  public Guid Id = Id;

  public Guid GroupId = GroupId;

  public int StudentGroupOrdinal = StudentGroupOrdinal;

  public StudentDetail ToStudentDetail()
  {
    return new StudentDetail(Id, GroupId, StudentGroupOrdinal);
  }
}
