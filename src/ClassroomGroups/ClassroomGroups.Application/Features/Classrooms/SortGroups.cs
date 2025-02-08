using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record SortGroupsRequest(Guid ClassroomId, Guid ConfigurationId, Guid[] SortedGroupIds)
  : IRequest<SortGroupsResponse>,
    IRequiredUserAccount { }

public record SortGroupsResponse(List<GroupDetail> SortedGroupDetails) { }

public class SortGroupsRequestHandler(
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext dbContext
) : IRequestHandler<SortGroupsRequest, SortGroupsResponse>
{
  public async Task<SortGroupsResponse> Handle(
    SortGroupsRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var groupOrdinalMap = request
        .SortedGroupIds.Select((id, index) => new { id, index })
        .ToDictionary(x => x.id, x => x.index);

      var groups = await dbContext
        .Groups.Where(g => request.SortedGroupIds.Contains(g.Id))
        .ToListAsync(cancellationToken);

      foreach (var group in groups)
      {
        if (groupOrdinalMap.TryGetValue(group.Id, out var ordinal))
        {
          group.Ordinal = ordinal;
        }
      }

      await transaction.CommitAsync(cancellationToken);

      await dbContext.SaveChangesAsync(cancellationToken);

      var groupDetails =
        await detailService.GetGroupDetails(
          account.Id,
          request.ClassroomId,
          request.ConfigurationId,
          cancellationToken
        ) ?? throw new Exception();

      return new SortGroupsResponse(groupDetails);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
