using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteGroupRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId)
  : IRequest<DeleteGroupResponse> { }

public record DeleteGroupResponse(Group DeletedGroup, GroupDetail UpdatedDefaultGroup) { }

public class DeleteGroupRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService,
  IOrdinalService ordinalService
) : IRequestHandler<DeleteGroupRequest, DeleteGroupResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  readonly IOrdinalService _ordinalService = ordinalService;

  public async Task<DeleteGroupResponse> Handle(
    DeleteGroupRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var groupDTO =
        await _dbContext
          .Groups.Where(g => g.Id == request.GroupId)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var displacedStudentGroups = await _dbContext
        .StudentGroups.Where(sg => sg.GroupId == groupDTO.Id)
        .ToListAsync(cancellationToken);

      var configurationDTO =
        await _dbContext
          .Configurations.Where(c => c.Id == groupDTO.ConfigurationId)
          .SingleOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var defaultGroupId = groupDTO.ConfigurationDTO.DefaultGroupId ?? Guid.Empty;
      var i = 0;
      var numStudentsInDefaultGroup = await _dbContext
        .StudentGroups.Where(sg => sg.GroupId == defaultGroupId)
        .CountAsync(cancellationToken);
      displacedStudentGroups.ForEach(sg =>
      {
        sg.GroupId = defaultGroupId;
        sg.Ordinal = numStudentsInDefaultGroup + i;
        i++;
      });

      var groupEntity = _dbContext.Groups.Remove(groupDTO);

      await _dbContext.SaveChangesAsync(cancellationToken);

      transaction.Commit();

      var defaultGroup =
        (
          await _detailService.GetGroupDetails(
            account.Id,
            request.ClassroomId,
            [defaultGroupId],
            cancellationToken
          )
        ).FirstOrDefault() ?? throw new Exception();

      return new DeleteGroupResponse(groupEntity.Entity.ToGroup(), defaultGroup);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
