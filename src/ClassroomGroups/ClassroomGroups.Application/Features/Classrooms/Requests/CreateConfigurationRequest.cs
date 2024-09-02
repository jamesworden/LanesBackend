using ClassroomGroups.Application.Features.Classrooms.Responses;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms.Requests;

public record CreateConfigurationRequest(string Label, Guid ClassroomId)
  : IRequest<CreateConfigurationResponse?>
{
  public string Label { get; set; } = Label;

  public Guid ClassroomId { get; set; } = ClassroomId;
}
