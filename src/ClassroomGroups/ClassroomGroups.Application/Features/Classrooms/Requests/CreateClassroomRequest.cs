using ClassroomGroups.Application.Features.Accounts.Responses;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms.Requests;

public record CreateClassroomRequest(string Label) : IRequest<CreateClassroomResponse?>
{
  public string Label { get; set; } = Label;
}
