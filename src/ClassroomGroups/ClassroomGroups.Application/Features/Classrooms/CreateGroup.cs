using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateGroupRequest(Guid ClassroomId, Guid ConfigurationId, string Label)
  : IRequest<CreateGroupResponse> { }

public record CreateGroupResponse(GroupDetail CreatedGroupDetail) { }

public class CreateGroupRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<CreateGroupRequest, CreateGroupResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  public async Task<CreateGroupResponse> Handle(
    CreateGroupRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var classroomIds = (
        await _dbContext
          .Classrooms.Where(c => c.AccountId == account.Id)
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.Id)
        .ToList();

      var configurationDTO =
        await _dbContext
          .Configurations.Where(c =>
            c.ClassroomId == request.ClassroomId
            && c.Id == request.ConfigurationId
            && classroomIds.Contains(c.ClassroomId)
          )
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var existingGroups = await _dbContext
        .Groups.Where(g => g.ConfigurationId == configurationDTO.Id)
        .Select(g => g.ToGroup())
        .ToListAsync(cancellationToken);

      var ordinal = existingGroups.Count;

      var label = request.Label ?? $"Group {ordinal + 1}";

      var groupId = Guid.NewGuid();

      var groupDTO = new GroupDTO()
      {
        Id = groupId,
        Label = label,
        ConfigurationId = request.ConfigurationId,
        Ordinal = ordinal,
        ConfigurationKey = configurationDTO.Key
      };
      var groupEntity = await _dbContext.Groups.AddAsync(groupDTO, cancellationToken);

      await _dbContext.SaveChangesAsync(cancellationToken);

      var groupDetails =
        await _detailService.GetGroupDetails(
          account.Id,
          request.ClassroomId,
          request.ConfigurationId,
          cancellationToken
        ) ?? throw new Exception();

      var groupDetail = groupDetails.Find(g => g.Id == groupId) ?? throw new Exception();

      transaction.Commit();

      return new CreateGroupResponse(groupDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
