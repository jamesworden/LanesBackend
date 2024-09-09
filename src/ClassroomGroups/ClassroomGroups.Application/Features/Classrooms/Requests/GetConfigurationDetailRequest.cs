using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms.Requests;

public record GetConfigurationDetailRequest(Guid ConfigurationId) : IRequest<ConfigurationDetail?>
{
  public Guid ConfigurationId { get; set; } = ConfigurationId;
}
