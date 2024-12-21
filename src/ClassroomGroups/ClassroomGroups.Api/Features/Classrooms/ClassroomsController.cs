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

  public record CreateClassroomRequestBody(string? Label, string? Description) { }

  [Authorize]
  [HttpPost()]
  public async Task<CreateClassroomResponse> CreateClassroom(
    [FromBody] CreateClassroomRequestBody body
  )
  {
    return await _mediator.Send(new CreateClassroomRequest(body.Label, body.Description));
  }

  public record CreateConfigurationRequestBody(string Label) { }

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

  public record PatchConfigurationRequestBody(string Label, string Description) { }

  [Authorize]
  [HttpPatch("{classroomId}/configurations/{configurationId}")]
  public async Task<PatchConfigurationResponse> PatchConfiguration(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromBody] PatchConfigurationRequestBody body
  )
  {
    return await _mediator.Send(
      new PatchConfigurationRequest(classroomId, configurationId, body.Label, body.Description)
    );
  }

  public record PatchClassroomRequestBody(string Label, string Description) { }

  [Authorize]
  [HttpPatch("{classroomId}")]
  public async Task<PatchClassroomResponse> PatchClassroom(
    [FromRoute] Guid classroomId,
    [FromBody] PatchClassroomRequestBody body
  )
  {
    return await _mediator.Send(
      new PatchClassroomRequest(classroomId, body.Label, body.Description)
    );
  }

  public record CreateGroupRequestBody(string Label = "") { }

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

  public record CreateStudentRequestBody(Guid ConfigurationId, Guid? GroupId) { }

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

  public record PatchGroupRequestBody(string Label = "") { }

  [Authorize]
  [HttpPatch("{classroomId}/configurations/{configurationId}/groups/{groupId}")]
  public async Task<PatchGroupResponse> PatchGroup(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromRoute] Guid groupId,
    [FromBody] PatchGroupRequestBody body
  )
  {
    return await _mediator.Send(
      new PatchGroupRequest(classroomId, configurationId, groupId, body.Label)
    );
  }

  [Authorize]
  [HttpDelete("{classroomId}/configurations/{configurationId}")]
  public async Task<DeleteConfigurationResponse> DeleteConfiguration(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId
  )
  {
    return await _mediator.Send(new DeleteConfigurationRequest(classroomId, configurationId));
  }

  public record CreateColumnRequestBody(string Label, FieldType Type) { }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/columns")]
  public async Task<CreateColumnResponse> CreateColumn(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromBody] CreateColumnRequestBody body
  )
  {
    return await _mediator.Send(
      new CreateColumnRequest(classroomId, configurationId, body.Label, body.Type)
    );
  }

  public record UpsertStudentFieldRequestBody(string Value) { }

  [Authorize]
  [HttpPut("{classroomId}/students/{studentId}/fields/{fieldId}")]
  public async Task<UpsertStudentFieldResponse> UpsertStudentField(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid studentId,
    [FromRoute] Guid fieldId,
    [FromBody] UpsertStudentFieldRequestBody body
  )
  {
    return await _mediator.Send(
      new UpsertStudentFieldRequest(classroomId, studentId, fieldId, body.Value)
    );
  }

  public record PatchFieldRequestBody(string Label = "") { }

  [Authorize]
  [HttpPatch("{classroomId}/fields/{fieldId}")]
  public async Task<PatchFieldResponse> PatchField(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid fieldId,
    [FromBody] PatchFieldRequestBody body
  )
  {
    return await _mediator.Send(new PatchFieldRequest(classroomId, fieldId, body.Label));
  }

  [Authorize]
  [HttpDelete("{classroomId}/students/{studentId}")]
  public async Task<DeleteStudentResponse> DeleteStudent(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid studentId
  )
  {
    return await _mediator.Send(new DeleteStudentRequest(classroomId, studentId));
  }

  public record SortGroupsRequestBody(Guid[] SortedGroupIds) { }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/sort-groups")]
  public async Task<SortGroupsResponse> SortGroups(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromBody] SortGroupsRequestBody body
  )
  {
    return await _mediator.Send(
      new SortGroupsRequest(classroomId, configurationId, body.SortedGroupIds)
    );
  }

  public record MoveStudentRequestBody(MoveStudentDetail MoveStudentDetail) { }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/move-student")]
  public async Task<MoveStudentResponse> MoveStudent(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromBody] MoveStudentRequestBody body
  )
  {
    return await _mediator.Send(
      new MoveStudentRequest(classroomId, configurationId, body.MoveStudentDetail)
    );
  }

  public record MoveColumnRequestBody(MoveColumnDetail MoveColumnDetail) { }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/columns/{columnId}/move")]
  public async Task<MoveColumnResponse> MoveColumn(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromRoute] Guid columnId,
    [FromBody] MoveColumnRequestBody body
  )
  {
    return await _mediator.Send(
      new MoveColumnRequest(classroomId, configurationId, columnId, body.MoveColumnDetail)
    );
  }

  [Authorize]
  [HttpDelete("{classroomId}/configurations/{configurationId}/columns/{columnId}")]
  public async Task<DeleteColumnResponse> DeleteColumn(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromRoute] Guid columnId
  )
  {
    return await _mediator.Send(new DeleteColumnRequest(classroomId, configurationId, columnId));
  }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/groups/{groupId}/lock")]
  public async Task<LockGroupResponse> LockGroup(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromRoute] Guid groupId
  )
  {
    return await _mediator.Send(new LockGroupRequest(classroomId, configurationId, groupId));
  }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/groups/{groupId}/unlock")]
  public async Task<UnlockGroupResponse> UnlockGroup(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromRoute] Guid groupId
  )
  {
    return await _mediator.Send(new UnlockGroupRequest(classroomId, configurationId, groupId));
  }

  public record GroupStudentsRequestBody(
    StudentGroupingStrategy Strategy,
    int? NumberOfGroups,
    int? StudentsPerGroup
  ) { }

  [Authorize]
  [HttpPost("{classroomId}/configurations/{configurationId}/group-students")]
  public async Task<GroupStudentsResponse> GroupStudents(
    [FromRoute] Guid classroomId,
    [FromRoute] Guid configurationId,
    [FromBody] GroupStudentsRequestBody body
  )
  {
    return await _mediator.Send(
      new GroupStudentsRequest(
        classroomId,
        configurationId,
        body.Strategy,
        body.NumberOfGroups,
        body.StudentsPerGroup
      )
    );
  }
}
