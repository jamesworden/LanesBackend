using ClassroomGroups.Application.Features.Classrooms;
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
  [HttpGet("{classroomId}/configuration-detail/{configurationId}")]
  public async Task<GetConfigurationDetailResponse> GetConfigurationDetail(
    [FromRoute] Guid configurationId,
    [FromRoute] Guid classroomId
  )
  {
    return await _mediator.Send(new GetConfigurationDetailRequest(classroomId, configurationId));
  }

  [Authorize]
  [HttpGet("classroom-details")]
  public async Task<GetClassroomDetailsResponse> GetClassroomDetails()
  {
    return await _mediator.Send(new GetClassroomDetailsRequest());
  }

  [Authorize]
  [HttpGet("{classroomId}/configurations")]
  public async Task<GetConfigurationsResponse> GetConfigurations([FromRoute] Guid classroomId)
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
  public async Task<DeleteClassroomResponse> DeleteClassroom([FromRoute] Guid classroomId)
  {
    return await _mediator.Send(new DeleteClassroomRequest(classroomId));
  }

  [Authorize]
  [HttpPost("{classroomId}/configurations")]
  public async Task<CreateConfigurationResponse> CreateConfiguration(
    [FromRoute] Guid classroomId,
    [FromBody] CreateConfigurationRequestBody body
  )
  {
    return await _mediator.Send(new CreateConfigurationRequest(body.Label, classroomId));
  }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}")]
  public async Task<PatchConfigurationResponse> PatchConfiguration(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromBody] PatchConfigurationRequestBody body
  )
  {
    return await _mediator.Send(
      new PatchConfigurationRequest(classroomId, configurationId, body.Configuration)
    );
  }

  [Authorize]
  [HttpPost("{classroomId}")]
  public async Task<PatchClassroomResponse> PatchClassroom(
    [FromRoute] Guid classroomId,
    [FromBody] PatchClassroomRequestBody body
  )
  {
    return await _mediator.Send(new PatchClassroomRequest(classroomId, body.Classroom));
  }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/groups")]
  public async Task<CreateGroupResponse> CreateGroup(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromBody] CreateGroupRequestBody body
  )
  {
    return await _mediator.Send(new CreateGroupRequest(classroomId, configurationId, body.Label));
  }

  [Authorize]
  [HttpDelete("{classroomId}/configurations/{configurationId}/groups/{groupId}")]
  public async Task<DeleteGroupResponse> DeleteGroup(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromRoute] Guid groupId
  )
  {
    return await _mediator.Send(new DeleteGroupRequest(classroomId, configurationId, groupId));
  }

  [Authorize]
  [HttpPost("{classroomId}/students")]
  public async Task<CreateStudentResponse> CreateStudent(
    [FromRoute] Guid classroomId,
    [FromBody] CreateStudentRequestBody body
  )
  {
    return await _mediator.Send(
      new CreateStudentRequest(classroomId, body.ConfigurationId, body.GroupId)
    );
  }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/groups/{groupId}")]
  public async Task<PatchGroupResponse> PatchGroup(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromRoute] Guid groupId,
    [FromBody] PatchGroupRequestBody body
  )
  {
    return await _mediator.Send(
      new PatchGroupRequest(classroomId, configurationId, groupId, body.Group)
    );
  }
}
