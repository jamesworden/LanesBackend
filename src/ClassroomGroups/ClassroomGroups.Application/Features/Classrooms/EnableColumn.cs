using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record EnableColumnRequest(Guid ClassroomId, Guid ConfigurationId, Guid ColumnId)
  : IRequest<EnableColumnResponse>,
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

public record EnableColumnResponse(Column EnabledColumn);

public class EnableColumnRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache
) : IRequestHandler<EnableColumnRequest, EnableColumnResponse>
{
  public async Task<EnableColumnResponse> Handle(
    EnableColumnRequest request,
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

      columnDTO.Enabled = true;

      await dbContext.SaveChangesAsync(cancellationToken);

      await transaction.CommitAsync(cancellationToken);

      return new EnableColumnResponse(columnDTO.ToColumn());
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
