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

  readonly string DEFAULT_GROUP_LABEL = "Default Group";

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

    var configurationId = Guid.NewGuid();

    var configurationDTO = new ConfigurationDTO
    {
      Id = configurationId,
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

    var defaultGroupDTO = new GroupDTO()
    {
      Id = Guid.NewGuid(),
      Label = DEFAULT_GROUP_LABEL,
      ConfigurationId = configurationId,
      Ordinal = 0,
      ConfigurationKey = configurationDTO.Key
    };
    var groupEntity = await _dbContext.Groups.AddAsync(defaultGroupDTO, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    configurationEntity.Entity.DefaultGroupKey = groupEntity.Entity.Key;
    configurationEntity.Entity.DefaultGroupId = groupEntity.Entity.Id;

    _dbContext.Configurations.Update(configurationEntity.Entity);

    await _dbContext.SaveChangesAsync(cancellationToken);

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
