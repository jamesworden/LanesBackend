using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.Application.Features.Classrooms.Responses;

public record CreateClassroomResponse(Classroom Classroom, Configuration Configuration)
{
  public Classroom CreatedClassroom { get; } = Classroom;

  public Configuration CreatedConfiguration { get; } = Configuration;
}
