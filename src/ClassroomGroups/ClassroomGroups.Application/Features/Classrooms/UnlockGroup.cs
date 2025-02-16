using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record UnlockGroupRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId)
  : IRequest<UnlockGroupResponse>,
    IRequiredUserAccount
{
  public EntityIds GetEntityIds() =>
    new()
    {
      ClassroomIds = [ClassroomId],
      ConfigurationIds = [ConfigurationId],
      GroupIds = [GroupId]
    };
}

public record UnlockGroupResponse(Group UpdatedGroup) { }

public class UnlockGroupRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache
) : IRequestHandler<UnlockGroupRequest, UnlockGroupResponse>
{
  public async Task<UnlockGroupResponse> Handle(
    UnlockGroupRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var classroomIds = (
        await dbContext
          .Classrooms.Where(c => c.AccountId == account.Id)
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.Id)
        .ToList();

      var groupDTO =
        await dbContext.Groups.FirstOrDefaultAsync(
          g =>
            g.Id == request.GroupId
            && g.ConfigurationId == request.ConfigurationId
            && classroomIds.Contains(g.ConfigurationDTO.ClassroomId)
            && g.IsLocked,
          cancellationToken
        ) ?? throw new InvalidOperationException();

      groupDTO.IsLocked = false;

      await dbContext.SaveChangesAsync(cancellationToken);

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
