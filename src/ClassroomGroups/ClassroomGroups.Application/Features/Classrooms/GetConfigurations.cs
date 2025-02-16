using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record GetConfigurationsRequest(Guid ClassroomId)
  : IRequest<GetConfigurationsResponse>,
    IRequiredUserAccount
{
  public EntityIds GetEntityIds() => new() { ClassroomIds = [ClassroomId] };
}

public record GetConfigurationsResponse(List<Configuration> Configurations) { }

public class GetConfigurationsRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache
) : IRequestHandler<GetConfigurationsRequest, GetConfigurationsResponse>
{
  public async Task<GetConfigurationsResponse> Handle(
    GetConfigurationsRequest request,
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
      ).Select(c => c.Id);

      var configurations = (
        await dbContext
          .Configurations.Where(c => classroomIds.Contains(c.ClassroomId))
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.ToConfiguration())
        .ToList();

      transaction.Commit();

      return new GetConfigurationsResponse(configurations);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
