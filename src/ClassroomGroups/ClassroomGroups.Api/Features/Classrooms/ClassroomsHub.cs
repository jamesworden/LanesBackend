using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace ClassroomGroups.Api.Features.Classrooms;

public class ClassroomsHub(IMediator mediator) : Hub
{
  private readonly IMediator _mediator = mediator;

  public override async Task OnDisconnectedAsync(Exception? _)
  {
    await Task.CompletedTask;
  }
}
