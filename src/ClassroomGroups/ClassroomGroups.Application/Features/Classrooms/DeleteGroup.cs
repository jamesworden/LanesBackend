using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteGroupRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId)
  : IRequest<DeleteGroupResponse> { }

public record DeleteGroupResponse(ConfigurationDetail UpdatedConfigurationDetail) { }

public class DeleteGroupRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IGetConfigurationDetailService getConfigurationDetailService
) : IRequestHandler<DeleteGroupRequest, DeleteGroupResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  readonly IGetConfigurationDetailService _getConfigurationDetailService =
    getConfigurationDetailService;

  public async Task<DeleteGroupResponse> Handle(
    DeleteGroupRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    var groupDTO =
      await _dbContext
        .Groups.Where(g => g.Id == request.GroupId)
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    var groupEntity = _dbContext.Groups.Remove(groupDTO);
    await _dbContext.SaveChangesAsync(cancellationToken);

    var configurationDetail =
      await _getConfigurationDetailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      ) ?? throw new Exception();

    return new DeleteGroupResponse(configurationDetail);
  }
}
