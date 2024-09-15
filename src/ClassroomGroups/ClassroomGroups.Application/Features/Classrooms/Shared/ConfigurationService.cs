using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Shared;

public interface IConfigurationService
{
  public Task<Configuration> CreateConfiguration(
    Guid accountId,
    Guid classroomId,
    string label,
    CancellationToken cancellationToken
  );
}

public class ConfigurationService(ClassroomGroupsContext dbContext) : IConfigurationService
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  public async Task<Configuration> CreateConfiguration(
    Guid accountId,
    Guid classroomId,
    string label,
    CancellationToken cancellationToken
  )
  {
    ClassroomDTO? classroomDTO =
      (
        await _dbContext.Classrooms.Where(c => c.Id == classroomId).ToListAsync(cancellationToken)
        ?? []
      ).FirstOrDefault() ?? throw new Exception();

    var configurationDTO = new ConfigurationDTO
    {
      Id = Guid.NewGuid(),
      Label = label,
      ClassroomId = classroomDTO.Id,
      ClassroomKey = classroomDTO.Key
    };
    var configurationEntity = await _dbContext.Configurations.AddAsync(
      configurationDTO,
      cancellationToken
    );
    await _dbContext.SaveChangesAsync(cancellationToken);
    var configuration = configurationEntity.Entity?.ToConfiguration() ?? throw new Exception();

    List<FieldDTO> fieldDTOs =
      await _dbContext
        .Fields.Where(f => f.ClassroomKey == classroomDTO.Key)
        .ToListAsync(cancellationToken) ?? [];

    List<ColumnDTO> columnDTOs = fieldDTOs
      .Select(
        (f, index) =>
          new ColumnDTO()
          {
            Id = Guid.NewGuid(),
            Enabled = true,
            Ordinal = index,
            ConfigurationId = configuration.Id,
            ConfigurationKey = configurationDTO.Key,
            Sort = ColumnSort.NONE,
            FieldId = f.Id,
            FieldKey = f.Key
          }
      )
      .ToList();

    await _dbContext.Columns.AddRangeAsync(columnDTOs, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);

    List<ColumnDTO> resultingColumnDTOs =
      await _dbContext
        .Columns.Where(col => columnDTOs.Select(c => c.Id).Contains(col.Id))
        .ToListAsync(cancellationToken) ?? [];

    return configurationDTO.ToConfiguration();
  }
}
