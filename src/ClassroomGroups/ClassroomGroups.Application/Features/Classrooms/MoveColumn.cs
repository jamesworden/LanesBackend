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
) : IRequest<MoveColumnResponse> { }

public record MoveColumnResponse(List<ColumnDetail> UpdatedColumnDetails) { }

public class MoveColumnRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext classroomGroupsContext
) : IRequestHandler<MoveColumnRequest, MoveColumnResponse>
{
  private readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;
  private readonly IDetailService _detailService = detailService;
  private readonly ClassroomGroupsContext _dbContext = classroomGroupsContext;

  public async Task<MoveColumnResponse> Handle(
    MoveColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var columns = await _dbContext
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

      await _dbContext.SaveChangesAsync(cancellationToken);
      await transaction.CommitAsync(cancellationToken);

      var updatedColumnDetails = await _detailService.GetColumnDetails(
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
