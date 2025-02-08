using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteGroupRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId)
  : IRequest<DeleteGroupResponse>,
    IRequiredUserAccount { }

public record DeleteGroupResponse(Group DeletedGroup, GroupDetail UpdatedDefaultGroup) { }

public class DeleteGroupRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService,
  IOrdinalService ordinalService
) : IRequestHandler<DeleteGroupRequest, DeleteGroupResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AccountRequiredCache _authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  readonly IOrdinalService _ordinalService = ordinalService;

  public async Task<DeleteGroupResponse> Handle(
    DeleteGroupRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;

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
        .OrderBy(sg => sg.Ordinal)
        .ToListAsync(cancellationToken);

      var configurationDTO =
        await _dbContext
          .Configurations.Where(c => c.Id == groupDTO.ConfigurationId)
          .SingleOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var defaultGroupId = groupDTO.ConfigurationDTO.DefaultGroupId ?? Guid.Empty;
      var defaultGroupKey = groupDTO.ConfigurationDTO.DefaultGroupKey ?? -1;

      var numStudentsInDefaultGroup = await _dbContext
        .StudentGroups.Where(sg => sg.GroupId == defaultGroupId)
        .CountAsync(cancellationToken);

      for (var i = 0; i < displacedStudentGroups.Count; i++)
      {
        _dbContext.Remove(displacedStudentGroups[i]);

        var studentGroupDTO = new StudentGroupDTO()
        {
          Id = Guid.NewGuid(),
          GroupId = defaultGroupId,
          GroupKey = defaultGroupKey,
          StudentId = displacedStudentGroups[i].StudentId,
          StudentKey = displacedStudentGroups[i].StudentKey,
          Ordinal = numStudentsInDefaultGroup + i
        };

        _dbContext.Add(studentGroupDTO);
      }

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
    catch (Exception e)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
