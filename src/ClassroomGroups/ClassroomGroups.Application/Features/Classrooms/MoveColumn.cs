using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record MoveColumnDetail(int PrevIndex, int CurrIndex) { }

public record MoveColumnRequest(
  Guid ClassroomId,
  Guid ConfigurationId,
  Guid ColumnId,
  MoveColumnDetail MoveColumnDetail
) : IRequest<MoveColumnResponse>, IRequiredUserAccount { }

public record MoveColumnResponse(List<ColumnDetail> UpdatedColumnDetails) { }

public class MoveColumnRequestHandler(
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext dbContext
) : IRequestHandler<MoveColumnRequest, MoveColumnResponse>
{
  public async Task<MoveColumnResponse> Handle(
    MoveColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var columns = await dbContext
        .Columns.Where(c => c.ConfigurationId == request.ConfigurationId)
        .OrderBy(c => c.Ordinal)
        .ToListAsync(cancellationToken);

      var column =
        columns.FirstOrDefault(c => c.Id == request.ColumnId)
        ?? throw new Exception("Column not found in the group");

      columns.Remove(column);
      columns.Insert(request.MoveColumnDetail.CurrIndex, column);

      for (int i = 0; i < columns.Count; i++)
      {
        columns[i].Ordinal = i;
      }

      await dbContext.SaveChangesAsync(cancellationToken);
      await transaction.CommitAsync(cancellationToken);

      var updatedColumnDetails = await detailService.GetColumnDetails(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      );

      return new MoveColumnResponse(updatedColumnDetails);
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
