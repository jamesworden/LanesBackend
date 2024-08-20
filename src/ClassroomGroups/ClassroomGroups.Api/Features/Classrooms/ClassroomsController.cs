using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClassroomGroups.Api.Features.Classrooms;

[ApiController]
[Route("classroom-groups/api/v1/[controller]")]
public class ClassroomsController(IMediator mediator) : ControllerBase
{
  private readonly IMediator _mediator = mediator;

  [Authorize]
  [HttpGet]
  public async Task<IEnumerable<Classroom>> GetClassrooms()
  {
    return await _mediator.Send(new GetClassroomsRequest());
  }
}
