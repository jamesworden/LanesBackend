using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record UnlockGroupRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId)
  : IRequest<UnlockGroupResponse> { }

public record UnlockGroupResponse(Group UpdatedGroup) { }

public class UnlockGroupRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache
) : IRequestHandler<UnlockGroupRequest, UnlockGroupResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<UnlockGroupResponse> Handle(
    UnlockGroupRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var classroomIds = (
        await _dbContext
          .Classrooms.Where(c => c.AccountId == account.Id)
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.Id)
        .ToList();

      var groupDTO =
        await _dbContext.Groups.FirstOrDefaultAsync(
          g =>
            g.Id == request.GroupId
            && g.ConfigurationId == request.ConfigurationId
            && classroomIds.Contains(g.ConfigurationDTO.ClassroomId)
            && g.IsLocked,
          cancellationToken
        ) ?? throw new Exception();

      groupDTO.IsLocked = false;

      await _dbContext.SaveChangesAsync(cancellationToken);

      transaction.Commit();

      return new UnlockGroupResponse(groupDTO.ToGroup());
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
