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
    IRequiredUserAccount
{
  public EntityIds GetEntityIds() =>
    new() { ClassroomIds = [ClassroomId], ConfigurationIds = [ConfigurationId] };
}

public record DeleteGroupResponse(Group DeletedGroup, GroupDetail UpdatedDefaultGroup) { }

public class DeleteGroupRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<DeleteGroupRequest, DeleteGroupResponse>
{
  public async Task<DeleteGroupResponse> Handle(
    DeleteGroupRequest request,
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
          .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      var displacedStudentGroups = await dbContext
        .StudentGroups.Where(sg => sg.GroupId == groupDTO.Id)
        .OrderBy(sg => sg.Ordinal)
        .ToListAsync(cancellationToken);

      var configurationDTO =
        await dbContext
          .Configurations.Where(c => c.Id == groupDTO.ConfigurationId)
          .SingleOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      var defaultGroupId = groupDTO.ConfigurationDTO.DefaultGroupId ?? Guid.Empty;
      var defaultGroupKey = groupDTO.ConfigurationDTO.DefaultGroupKey ?? -1;

      var numStudentsInDefaultGroup = await dbContext
        .StudentGroups.Where(sg => sg.GroupId == defaultGroupId)
        .CountAsync(cancellationToken);

      for (var i = 0; i < displacedStudentGroups.Count; i++)
      {
        dbContext.Remove(displacedStudentGroups[i]);

        var studentGroupDTO = new StudentGroupDTO()
        {
          Id = Guid.NewGuid(),
          GroupId = defaultGroupId,
          GroupKey = defaultGroupKey,
          StudentId = displacedStudentGroups[i].StudentId,
          StudentKey = displacedStudentGroups[i].StudentKey,
          Ordinal = numStudentsInDefaultGroup + i
        };

        dbContext.Add(studentGroupDTO);
      }

      var groupEntity = dbContext.Groups.Remove(groupDTO);

      await dbContext.SaveChangesAsync(cancellationToken);

      transaction.Commit();

      var defaultGroup =
        (
          await detailService.GetGroupDetails(
            account.Id,
            request.ClassroomId,
            [defaultGroupId],
            cancellationToken
          )
        ).FirstOrDefault() ?? throw new InvalidOperationException();

      return new DeleteGroupResponse(groupEntity.Entity.ToGroup(), defaultGroup);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
