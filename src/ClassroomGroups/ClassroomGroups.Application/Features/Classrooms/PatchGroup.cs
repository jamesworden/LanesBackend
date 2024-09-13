using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record PatchGroupRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId, Group Group)
  : IRequest<PatchGroupResponse> { }

public record PatchGroupResponse(GroupDetail UpdatedGroupDetail) { }

public class PatchGroupRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext classroomGroupsContext
) : IRequestHandler<PatchGroupRequest, PatchGroupResponse>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  readonly ClassroomGroupsContext _dbContext = classroomGroupsContext;

  public async Task<PatchGroupResponse> Handle(
    PatchGroupRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    var groupDTO =
      await _dbContext
        .Groups.Where(g => g.Id == request.GroupId)
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    groupDTO.Label = request.Group.Label;

    var configurationEntity = _dbContext.Groups.Update(groupDTO);
    await _dbContext.SaveChangesAsync(cancellationToken);

    var groupDetails = await _detailService.GetGroupDetails(
      account.Id,
      request.ClassroomId,
      request.ConfigurationId,
      cancellationToken
    );

    var groupDetail =
      groupDetails.FirstOrDefault(g => g.Id == groupDTO.Id) ?? throw new Exception();

    return new PatchGroupResponse(groupDetail);
  }
}
