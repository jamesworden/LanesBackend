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
) : IRequest<GroupStudentsResponse>, IRequiredUserAccount;

public record GroupStudentsResponse(
  List<GroupDetail> UpdatedGroupDetails,
  string? ErrorMessage = null
);

public class GroupStudentsRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<GroupStudentsRequest, GroupStudentsResponse>
{
  public async Task<GroupStudentsResponse> Handle(
    GroupStudentsRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new UnauthorizedAccessException();

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var result = await ProcessGrouping(request, account.Id, cancellationToken);
      if (result.ErrorMessage is not null)
      {
        return result;
      }

      await transaction.CommitAsync(cancellationToken);
      return result;
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }

  private async Task<GroupStudentsResponse> ProcessGrouping(
    GroupStudentsRequest request,
    Guid accountId,
    CancellationToken cancellationToken
  )
  {
    var (configurationDetail, fields) = await GetInitialData(request, accountId, cancellationToken);

    var (groupingResult, errorMessage) = configurationDetail.GroupStudents(
      fields,
      request.Strategy,
      request.NumberOfGroups,
      request.StudentsPerGroup
    );

    if (errorMessage is not null)
    {
      return new GroupStudentsResponse([], errorMessage);
    }

    await UpdateDatabase(groupingResult, configurationDetail.Id, cancellationToken);

    var updatedGroupDetails = await detailService.GetGroupDetails(
      accountId,
      request.ClassroomId,
      request.ConfigurationId,
      cancellationToken
    );

    return new GroupStudentsResponse(updatedGroupDetails);
  }

  private async Task<(ConfigurationDetail, List<Field>)> GetInitialData(
    GroupStudentsRequest request,
    Guid accountId,
    CancellationToken cancellationToken
  )
  {
    var configurationDetail = await detailService.GetConfigurationDetail(
      accountId,
      request.ClassroomId,
      request.ConfigurationId,
      cancellationToken
    );

    var fields = await dbContext
      .Fields.Where(f => f.ClassroomId == request.ClassroomId)
      .Select(f => f.ToField())
      .ToListAsync(cancellationToken);

    return (configurationDetail, fields);
  }

  private async Task UpdateDatabase(
    GroupStudentsResultDetails result,
    Guid configurationId,
    CancellationToken cancellationToken
  )
  {
    var configurationDTO = await GetConfiguration(configurationId, cancellationToken);
    await CreateNewGroups(result.GroupsToCreate, configurationDTO, cancellationToken);
    await DeleteExistingStudentGroups(result.StudentGroupIdsToDelete, cancellationToken);
    await CreateNewStudentGroups(result, configurationDTO.Id, cancellationToken);
    await DeleteUnusedGroups(result.GroupIdsToDelete, cancellationToken);
  }

  private async Task<ConfigurationDTO> GetConfiguration(
    Guid configurationId,
    CancellationToken cancellationToken
  )
  {
    return await dbContext
        .Configurations.Where(c => c.Id == configurationId)
        .FirstOrDefaultAsync(cancellationToken)
      ?? throw new InvalidOperationException("Configuration not found");
  }

  private async Task CreateNewGroups(
    IEnumerable<Group> groupsToCreate,
    ConfigurationDTO configurationDTO,
    CancellationToken cancellationToken
  )
  {
    var groupDTOs = groupsToCreate.Select(g => new GroupDTO
    {
      ConfigurationId = configurationDTO.Id,
      ConfigurationKey = configurationDTO.Key,
      Id = g.Id,
      IsLocked = g.IsLocked,
      Label = g.Label,
      Ordinal = g.Ordinal,
    });

    await dbContext.Groups.AddRangeAsync(groupDTOs, cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task DeleteExistingStudentGroups(
    IEnumerable<Guid> studentGroupIdsToDelete,
    CancellationToken cancellationToken
  )
  {
    var studentGroupsToDelete = await dbContext
      .StudentGroups.Where(sg => studentGroupIdsToDelete.Contains(sg.Id))
      .ToListAsync(cancellationToken);

    dbContext.StudentGroups.RemoveRange(studentGroupsToDelete);
    await dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task CreateNewStudentGroups(
    GroupStudentsResultDetails result,
    Guid configurationId,
    CancellationToken cancellationToken
  )
  {
    var groupIdsToKeys = await GetGroupKeyMappings(configurationId);
    var studentIdsToKeys = await GetStudentKeyMappings(result.StudentGroupsToCreate);

    var studentGroupDTOs = result.StudentGroupsToCreate.Select(sg => new StudentGroupDTO
    {
      Id = sg.Id,
      GroupId = sg.GroupId,
      GroupKey = groupIdsToKeys[sg.GroupId],
      StudentId = sg.StudentId,
      StudentKey = studentIdsToKeys[sg.StudentId],
      Ordinal = sg.Ordinal,
    });

    dbContext.StudentGroups.AddRange(studentGroupDTOs);
    await dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task<Dictionary<Guid, int>> GetGroupKeyMappings(Guid configurationId)
  {
    return await dbContext
      .Groups.Where(g => g.ConfigurationId == configurationId)
      .ToDictionaryAsync(g => g.Id, g => g.Key);
  }

  private async Task<Dictionary<Guid, int>> GetStudentKeyMappings(
    IEnumerable<StudentGroup> studentGroups
  )
  {
    var relevantStudentIds = studentGroups.Select(sg => sg.StudentId);
    return await dbContext
      .Students.Where(s => relevantStudentIds.Contains(s.Id))
      .ToDictionaryAsync(s => s.Id, s => s.Key);
  }

  private async Task DeleteUnusedGroups(
    IEnumerable<Guid> groupIdsToDelete,
    CancellationToken cancellationToken
  )
  {
    var unusedGroups = await dbContext
      .Groups.Where(g => groupIdsToDelete.Contains(g.Id))
      .ToListAsync(cancellationToken);

    dbContext.Groups.RemoveRange(unusedGroups);
    await dbContext.SaveChangesAsync(cancellationToken);
  }
}
