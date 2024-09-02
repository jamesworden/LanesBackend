using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.Application.Features.Classrooms.Responses;

public record CreateConfigurationResponse(Configuration Configuration)
{
  public Configuration CreatedConfiguration { get; } = Configuration;
}
