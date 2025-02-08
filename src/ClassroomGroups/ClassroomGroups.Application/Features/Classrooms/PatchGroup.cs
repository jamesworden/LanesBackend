using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record PatchGroupRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId, string Label)
  : IRequest<PatchGroupResponse>,
    IRequiredUserAccount { }

public record PatchGroupResponse(GroupDetail UpdatedGroupDetail) { }

public class PatchGroupRequestHandler(
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext dbContext
) : IRequestHandler<PatchGroupRequest, PatchGroupResponse>
{
  public async Task<PatchGroupResponse> Handle(
    PatchGroupRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var groupDTO =
        await dbContext
          .Groups.Where(g => g.Id == request.GroupId)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      groupDTO.Label = request.Label;

      var configurationEntity = dbContext.Groups.Update(groupDTO);
      await dbContext.SaveChangesAsync(cancellationToken);

      var groupDetails = await detailService.GetGroupDetails(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      );

      var groupDetail =
        groupDetails.FirstOrDefault(g => g.Id == groupDTO.Id) ?? throw new Exception();

      transaction.Commit();

      return new PatchGroupResponse(groupDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
