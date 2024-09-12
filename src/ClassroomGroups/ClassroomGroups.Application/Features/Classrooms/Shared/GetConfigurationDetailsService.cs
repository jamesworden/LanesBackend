using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Shared;

public interface IGetConfigurationDetailService
{
  public Task<ConfigurationDetail> GetConfigurationDetail(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  );
}

public class GetConfigurationDetailService(ClassroomGroupsContext dbContext)
  : IGetConfigurationDetailService
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  public async Task<ConfigurationDetail> GetConfigurationDetail(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  )
  {
    var studentDetails =
      (
        await _dbContext
          .StudentGroups.Where(sg => sg.GroupDTO.ConfigurationId == configurationId)
          .Select(sg => new StudentDetailDTO(sg.Id, sg.GroupId, sg.Ordinal))
          .ToListAsync(cancellationToken)
      )
        .Select(sg => sg.ToStudentDetail())
        .ToList() ?? [];

    var groupDetails =
      (
        await _dbContext
          .Groups.Where(g => g.ConfigurationId == configurationId)
          .Select(g => new GroupDetailDTO(g.Id, g.ConfigurationId, g.Label, g.Ordinal))
          .ToListAsync(cancellationToken)
      )
        .Select(g => g.ToGroupDetail(studentDetails))
        .ToList() ?? [];

    var columnDetails = (
      await _dbContext
        .Columns.Where(c => c.Id == configurationId)
        .Join(
          _dbContext.Fields,
          column => column.FieldId,
          field => field.Id,
          (column, field) => new { column, field }
        )
        .Select(c => new ColumnDetailDTO(
          c.column.Id,
          c.column.ConfigurationId,
          c.column.FieldId,
          c.column.Ordinal,
          c.column.Sort,
          c.column.Enabled,
          c.field.Type
        ))
        .ToListAsync(cancellationToken)
    )
      .Select(c => c.ToColumnDetail())
      .ToList();

    var classroomIds = (
      await _dbContext.Classrooms.Where(c => c.AccountId == accountId).ToListAsync()
    )
      .Select(c => c.Id)
      .ToList();

    var configurationDetail =
      (
        await _dbContext
          .Configurations.Where(c =>
            c.Id == configurationId
            && c.ClassroomId == classroomId
            && classroomIds.Contains(classroomId)
          )
          .Select(c => new ConfigurationDetailDTO(c.Id, c.ClassroomId, c.Label, c.Description))
          .FirstOrDefaultAsync(cancellationToken)
      )?.ToConfigurationDetail(groupDetails, columnDetails) ?? throw new Exception();

    return configurationDetail;
  }
}
