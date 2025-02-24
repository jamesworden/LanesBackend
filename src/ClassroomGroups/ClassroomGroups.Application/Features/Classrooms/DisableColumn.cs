using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DisableColumnRequest(Guid ClassroomId, Guid ConfigurationId, Guid ColumnId)
  : IRequest<DisableColumnResponse>,
    IRequiredUserAccount
{
  public EntityIds GetEntityIds() =>
    new()
    {
      ClassroomIds = [ClassroomId],
      ConfigurationIds = [ConfigurationId],
      ColumnIds = [ColumnId]
    };
}

public record DisableColumnResponse(Column DisabledColumn);

public class DisableColumnRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache
) : IRequestHandler<DisableColumnRequest, DisableColumnResponse>
{
  public async Task<DisableColumnResponse> Handle(
    DisableColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var columnDTO =
        await dbContext
          .Columns.Include(c => c.ConfigurationDTO)
          .Where(c =>
            c.Id == request.ColumnId && c.ConfigurationDTO.ClassroomId == request.ClassroomId
          )
          .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      columnDTO.Enabled = false;

      await dbContext.SaveChangesAsync(cancellationToken);

      await transaction.CommitAsync(cancellationToken);

      return new DisableColumnResponse(columnDTO.ToColumn());
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
