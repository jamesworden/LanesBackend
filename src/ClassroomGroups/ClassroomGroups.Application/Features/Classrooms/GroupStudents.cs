using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record GroupStudentsRequest(
  Guid ClassroomId,
  Guid ConfigurationId,
  StudentGroupingStrategy Strategy,
  int? NumberOfGroups = null,
  int? StudentsPerGroup = null
) : IRequest<GroupStudentsResponse>;

public record GroupStudentsResponse(List<GroupDetail> UpdatedGroupDetails);

public class GroupStudentsRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<GroupStudentsRequest, GroupStudentsResponse>
{
  private readonly ClassroomGroupsContext _dbContext = dbContext;
  private readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;
  private readonly IDetailService _detailService = detailService;

  public async Task<GroupStudentsResponse> Handle(
    GroupStudentsRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new UnauthorizedAccessException();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var configurationDetail = await _detailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      );

      var fields = await _dbContext
        .Fields.Where(f => f.ClassroomId == request.ClassroomId)
        .Select(f => f.ToField())
        .ToListAsync(cancellationToken);

      var result = configurationDetail.GroupStudents(
        fields,
        request.Strategy,
        request.NumberOfGroups,
        request.StudentsPerGroup
      );

      var configurationDTO =
        await _dbContext
          .Configurations.Where(c => c.Id == configurationDetail.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var groupDTOsToCreate = result.GroupsToCreate.Select(g => new GroupDTO()
      {
        ConfigurationId = configurationDetail.Id,
        ConfigurationKey = configurationDTO.Key,
        Id = g.Id,
        IsLocked = g.IsLocked,
        Label = g.Label,
        Ordinal = g.Ordinal,
      });

      await _dbContext.Groups.AddRangeAsync(groupDTOsToCreate, cancellationToken);
      await _dbContext.SaveChangesAsync(cancellationToken);

      var studentGroupDTOsToDelete = await _dbContext
        .StudentGroups.Where(sg => result.StudentGroupIdsToDelete.Contains(sg.Id))
        .ToListAsync(cancellationToken);

      _dbContext.StudentGroups.RemoveRange(studentGroupDTOsToDelete);
      await _dbContext.SaveChangesAsync(cancellationToken);

      var relevantGroupDTOs = _dbContext.Groups.Where(g =>
        g.ConfigurationId == configurationDTO.Id
      );
      var groupIdsToKeys = relevantGroupDTOs.ToDictionary((g) => g.Id, g => g.Key);

      var relevantStudentIds = result.StudentGroupsToCreate.Select(sg => sg.StudentId);
      var relevantStudentDTOs = _dbContext.Students.Where(s => relevantStudentIds.Contains(s.Id));
      var studentIdsToKeys = relevantStudentDTOs.ToDictionary(s => s.Id, s => s.Key);

      var studentGroupDTOsToCreate = result.StudentGroupsToCreate.Select(sg => new StudentGroupDTO()
      {
        Id = sg.Id,
        GroupId = sg.GroupId,
        GroupKey = groupIdsToKeys[sg.GroupId],
        StudentId = sg.StudentId,
        StudentKey = studentIdsToKeys[sg.StudentId],
        Ordinal = sg.Ordinal,
      });

      _dbContext.StudentGroups.AddRange(studentGroupDTOsToCreate);
      await _dbContext.SaveChangesAsync(cancellationToken);

      var unusedGroupDTOs = await _dbContext
        .Groups.Where(g => result.UnpopulatedGroupIds.Contains(g.Id))
        .ToListAsync(cancellationToken);

      _dbContext.Groups.RemoveRange(unusedGroupDTOs);

      await _dbContext.SaveChangesAsync(cancellationToken);

      await transaction.CommitAsync(cancellationToken);

      var groupDetails = await _detailService.GetGroupDetails(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      );

      return new GroupStudentsResponse(groupDetails);
    }
    catch (Exception e)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
