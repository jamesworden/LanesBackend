using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateConfigurationRequest(string Label, Guid ClassroomId)
  : IRequest<CreateConfigurationResponse> { }

public record CreateConfigurationResponse(ConfigurationDetail CreatedConfigurationDetail) { }

public class CreateConfigurationRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IGetConfigurationDetailService getConfigurationDetailService
) : IRequestHandler<CreateConfigurationRequest, CreateConfigurationResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  readonly IGetConfigurationDetailService _getConfigurationDetailService =
    getConfigurationDetailService;

  public async Task<CreateConfigurationResponse> Handle(
    CreateConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    ClassroomDTO? classroomDTO =
      (
        await _dbContext
          .Classrooms.Where(c => c.Id == request.ClassroomId)
          .ToListAsync(cancellationToken) ?? []
      ).FirstOrDefault() ?? throw new Exception();

    var configurationDTO = new ConfigurationDTO
    {
      Id = Guid.NewGuid(),
      Label = request.Label,
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

    var configurationDetail =
      await _getConfigurationDetailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        configuration.Id,
        cancellationToken
      ) ?? throw new Exception();

    return new CreateConfigurationResponse(configurationDetail);
  }
}
