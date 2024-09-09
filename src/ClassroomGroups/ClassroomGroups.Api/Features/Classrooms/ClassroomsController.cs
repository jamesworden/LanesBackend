using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Application.Features.Classrooms.Responses;
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
  [HttpGet("{classroomId}/configuration-detail/{configurationId}")]
  public async Task<ConfigurationDetail?> GetConfigurationDetail(
    [FromRoute] Guid configurationId,
    [FromRoute] Guid classroomId
  )
  {
    return await _mediator.Send(new GetConfigurationDetailRequest(classroomId, configurationId));
  }

  [Authorize]
  [HttpGet("classroom-details")]
  public async Task<List<ClassroomDetail>?> GetClassroomDetails()
  {
    return await _mediator.Send(new GetClassroomDetailRequest());
  }

  [Authorize]
  [HttpPost()]
  public async Task<CreateClassroomResponse?> CreateClassroom(
    [FromBody] CreateClassroomRequest request
  )
  {
    return await _mediator.Send(request);
  }

  [Authorize]
  [HttpDelete("classrooms/{classroomId}")]
  public async Task<DeleteClassroomResponse?> DeleteClassroom([FromRoute] Guid classroomId)
  {
    return await _mediator.Send(new DeleteClassroomRequest(classroomId));
  }

  [Authorize]
  [HttpPost("api/v1/classrooms/{classroomId}/configurations")]
  public async Task<CreateConfigurationResponse?> CreateConfiguration(
    [FromRoute] Guid classroomId,
    [FromBody] CreateConfigurationRequest request
  )
  {
    request.ClassroomId = classroomId;
    return await _mediator.Send(request);
  }
}
