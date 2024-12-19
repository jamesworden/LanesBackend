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

      var (updatedStudentGroups, createdGroups) = configurationDetail.GroupStudents(
        fields,
        request.Strategy,
        request.NumberOfGroups,
        request.StudentsPerGroup
      );

      var configurationDTO =
        await _dbContext
          .Configurations.Where(c => c.Id == configurationDetail.Id)
          .FirstOrDefaultAsync() ?? throw new Exception();

      var createGroupDTOs = createdGroups.Select(g => new GroupDTO()
      {
        ConfigurationId = configurationDetail.Id,
        ConfigurationKey = configurationDTO.Key,
        Id = g.Id,
        IsLocked = g.IsLocked,
        Label = g.Label,
        Ordinal = g.Ordinal,
      });

      await _dbContext.Groups.AddRangeAsync(createGroupDTOs, cancellationToken);
      await _dbContext.SaveChangesAsync(cancellationToken);

      var studentDTOs = await _dbContext
        .Students.Where(s => s.ClassroomId == request.ClassroomId)
        .ToListAsync(cancellationToken);

      var studentGroupsByIds = updatedStudentGroups.ToDictionary(sg => sg.Id);

      var studentIds = studentDTOs.Select(s => s.Id);

      var existingStudentGroupDTOs = await _dbContext
        .StudentGroups.Where(sg => studentIds.Contains(sg.StudentId))
        .ToListAsync(cancellationToken);

      var groupDTOs = await _dbContext
        .Groups.Where(g => g.ConfigurationId == request.ConfigurationId)
        .ToListAsync(cancellationToken);

      var groupDTOsByIds = groupDTOs.ToDictionary(g => g.Id);

      var studentDTOsByIds = studentDTOs.ToDictionary(g => g.Id);

      var updatedStudentGroupDTOs = existingStudentGroupDTOs.Select(sg =>
      {
        if (studentGroupsByIds.TryGetValue(sg.Id, out var studentGroup))
        {
          sg.GroupId = studentGroup.GroupId;
          sg.GroupKey = groupDTOsByIds[studentGroup.GroupId].Key;
          sg.StudentId = studentGroup.StudentId;
          sg.StudentKey = studentDTOsByIds[studentGroup.StudentId].Key;
          sg.Ordinal = sg.Ordinal;
        }
        return sg;
      });

      _dbContext.StudentGroups.UpdateRange(updatedStudentGroupDTOs);

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
