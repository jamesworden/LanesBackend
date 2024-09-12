using ClassroomGroups.Application.Features.Classrooms;
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
  [HttpGet("{classroomId}/configuration-detail/{configurationId}")]
  public async Task<GetConfigurationDetailResponse?> GetConfigurationDetail(
    [FromRoute] Guid configurationId,
    [FromRoute] Guid classroomId
  )
  {
    return await _mediator.Send(new GetConfigurationDetailRequest(classroomId, configurationId));
  }

  [Authorize]
  [HttpGet("classroom-details")]
  public async Task<GetClassroomDetailsResponse?> GetClassroomDetails()
  {
    return await _mediator.Send(new GetClassroomDetailsRequest());
  }

  [Authorize]
  [HttpGet("{classroomId}/configurations")]
  public async Task<GetConfigurationsResponse?> GetConfigurations([FromRoute] Guid classroomId)
  {
    return await _mediator.Send(new GetConfigurationsRequest(classroomId));
  }

  [Authorize]
  [HttpPost()]
  public async Task<CreateClassroomResponse> CreateClassroom(
    [FromBody] CreateClassroomRequestBody body
  )
  {
    return await _mediator.Send(new CreateClassroomRequest(body.Label, body.Description));
  }

  [Authorize]
  [HttpDelete("{classroomId}")]
  public async Task<DeleteClassroomResponse?> DeleteClassroom([FromRoute] Guid classroomId)
  {
    return await _mediator.Send(new DeleteClassroomRequest(classroomId));
  }

  [Authorize]
  [HttpPost("{classroomId}/configurations")]
  public async Task<CreateConfigurationResponse?> CreateConfiguration(
    [FromRoute] Guid classroomId,
    [FromBody] CreateConfigurationRequestBody body
  )
  {
    return await _mediator.Send(new CreateConfigurationRequest(body.Label, classroomId));
  }
}
