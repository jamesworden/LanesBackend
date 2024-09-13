using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record PatchGroupRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId, Group Group)
  : IRequest<PatchGroupResponse> { }

public record PatchGroupResponse(ConfigurationDetail UpdatedConfigurationDetail) { }

public class PatchGroupRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  IGetDetailService getConfigurationDetailService,
  ClassroomGroupsContext classroomGroupsContext
) : IRequestHandler<PatchGroupRequest, PatchGroupResponse>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly IGetDetailService _getConfigurationDetailService = getConfigurationDetailService;

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

    var configurationDetail =
      await _getConfigurationDetailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      ) ?? throw new Exception();

    return new PatchGroupResponse(configurationDetail);
  }
}
