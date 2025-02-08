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
        await dbContext.Classrooms.Where(c => c.Id == classroomId).ToListAsync(cancellationToken)
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
    var configurationEntity = await dbContext.Configurations.AddAsync(
      configurationDTO,
      cancellationToken
    );
    await dbContext.SaveChangesAsync(cancellationToken);
    var configuration = configurationEntity.Entity?.ToConfiguration() ?? throw new Exception();

    var defaultGroupDTO = new GroupDTO()
    {
      Id = Guid.NewGuid(),
      Label = DEFAULT_GROUP_LABEL,
      ConfigurationId = configurationId,
      Ordinal = 0,
      ConfigurationKey = configurationDTO.Key
    };
    var defaultGroupEntity = await dbContext.Groups.AddAsync(defaultGroupDTO, cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);

    configurationEntity.Entity.DefaultGroupKey = defaultGroupEntity.Entity.Key;
    configurationEntity.Entity.DefaultGroupId = defaultGroupEntity.Entity.Id;

    dbContext.Configurations.Update(configurationEntity.Entity);

    await dbContext.SaveChangesAsync(cancellationToken);

    List<FieldDTO> fieldDTOs =
      await dbContext
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

    await dbContext.Columns.AddRangeAsync(columnDTOs, cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);

    List<ColumnDTO> resultingColumnDTOs =
      await dbContext
        .Columns.Where(col => columnDTOs.Select(c => c.Id).Contains(col.Id))
        .ToListAsync(cancellationToken) ?? [];

    var studentDTOs =
      await dbContext.Students.Where(s => s.ClassroomId == classroomId).ToListAsync()
      ?? throw new Exception();

    var i = 0;

    var studentGroups = studentDTOs
      .Select(
        (studentDTO) =>
        {
          return new StudentGroupDTO()
          {
            GroupId = defaultGroupEntity.Entity.Id,
            GroupKey = defaultGroupEntity.Entity.Key,
            StudentId = studentDTO.Id,
            StudentKey = studentDTO.Key,
            Ordinal = i++,
            Id = Guid.NewGuid(),
          };
        }
      )
      .ToList();

    foreach (var studentGroup in studentGroups)
    {
      var existingEntity = dbContext.StudentGroups.FirstOrDefault(sg => sg.Id == studentGroup.Id);

      if (existingEntity != null)
      {
        existingEntity.GroupId = studentGroup.GroupId;
        existingEntity.GroupKey = studentGroup.GroupKey;
        existingEntity.StudentId = studentGroup.StudentId;
        existingEntity.StudentKey = studentGroup.StudentKey;
        existingEntity.Ordinal = studentGroup.Ordinal;

        dbContext.StudentGroups.Update(existingEntity);
      }
      else
      {
        dbContext.StudentGroups.Add(studentGroup);
      }
    }

    dbContext.SaveChanges();

    return configurationDTO.ToConfiguration();
  }
}
