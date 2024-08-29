using ClassroomGroups.Application.Features.Accounts.Responses;
using MediatR;

namespace ClassroomGroups.Application.Features.Accounts.Requests;

public record CreateClassroomRequest(string Label) : IRequest<CreateClassroomResponse?>
{
  public string Label { get; set; } = Label;
}
