using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.Application.Features.Classrooms.Responses;

public record DeleteClassroomResponse(Classroom Classroom)
{
  public Classroom DeletedClassroom { get; } = Classroom;
}
