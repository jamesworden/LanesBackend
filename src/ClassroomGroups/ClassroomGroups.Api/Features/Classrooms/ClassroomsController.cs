using ClassroomGroups.Application.Features.Accounts.Requests;
using ClassroomGroups.Application.Features.Accounts.Responses;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
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
  [HttpGet("classroom-details")]
  public async Task<ClassroomDetails?> GetClassroomDetails()
  {
    return await _mediator.Send(new GetClassroomDetailsRequest());
  }

  [Authorize]
  [HttpPost()]
  public async Task<CreateClassroomResponse?> CreateClassroom(
    [FromBody] CreateClassroomRequest request
  )
  {
    return await _mediator.Send(request);
  }
}
