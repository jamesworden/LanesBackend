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
) : IRequest<GroupStudentsResponse>, IRequiredUserAccount { };

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
      var configurationDetail = await detailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      );

      var fields = await dbContext
        .Fields.Where(f => f.ClassroomId == request.ClassroomId)
        .Select(f => f.ToField())
        .ToListAsync(cancellationToken);

      var (result, errorMessage) = configurationDetail.GroupStudents(
        fields,
        request.Strategy,
        request.NumberOfGroups,
        request.StudentsPerGroup
      );

      if (errorMessage is not null)
      {
        return new GroupStudentsResponse([], errorMessage);
      }

      var configurationDTO =
        await dbContext
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

      await dbContext.Groups.AddRangeAsync(groupDTOsToCreate, cancellationToken);
      await dbContext.SaveChangesAsync(cancellationToken);

      var studentGroupDTOsToDelete = await dbContext
        .StudentGroups.Where(sg => result.StudentGroupIdsToDelete.Contains(sg.Id))
        .ToListAsync(cancellationToken);

      dbContext.StudentGroups.RemoveRange(studentGroupDTOsToDelete);
      await dbContext.SaveChangesAsync(cancellationToken);

      var relevantGroupDTOs = dbContext.Groups.Where(g => g.ConfigurationId == configurationDTO.Id);
      var groupIdsToKeys = relevantGroupDTOs.ToDictionary((g) => g.Id, g => g.Key);

      var relevantStudentIds = result.StudentGroupsToCreate.Select(sg => sg.StudentId);
      var relevantStudentDTOs = dbContext.Students.Where(s => relevantStudentIds.Contains(s.Id));
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

      dbContext.StudentGroups.AddRange(studentGroupDTOsToCreate);
      await dbContext.SaveChangesAsync(cancellationToken);

      var unusedGroupDTOs = await dbContext
        .Groups.Where(g => result.UnpopulatedGroupIds.Contains(g.Id))
        .ToListAsync(cancellationToken);

      dbContext.Groups.RemoveRange(unusedGroupDTOs);

      await dbContext.SaveChangesAsync(cancellationToken);

      await transaction.CommitAsync(cancellationToken);

      var groupDetails = await detailService.GetGroupDetails(
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
