using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateConfigurationRequestBody(string Label) { }

public record CreateConfigurationRequest(string Label, Guid ClassroomId)
  : IRequest<CreateConfigurationResponse?>
{
  public string Label { get; set; } = Label;

  public Guid ClassroomId { get; set; } = ClassroomId;
}

public record CreateConfigurationResponse(ConfigurationDetail CreatedConfigurationDetail) { }

public class CreateConfigurationRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IMediator mediator
) : IRequestHandler<CreateConfigurationRequest, CreateConfigurationResponse?>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  readonly IMediator _mediator = mediator;

  public async Task<CreateConfigurationResponse?> Handle(
    CreateConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    ClassroomDTO? classroomDTO = (
      await _dbContext
        .Classrooms.Where(c => c.Id == request.ClassroomId)
        .ToListAsync(cancellationToken) ?? []
    ).FirstOrDefault();

    if (classroomDTO is null)
    {
      return null;
    }

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
    var configuration = configurationEntity.Entity?.ToConfiguration();
    if (configuration is null)
    {
      return null;
    }

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

    var configurationDetail = (
      await _mediator.Send(
        new GetConfigurationDetailRequest(configuration.ClassroomId, configuration.Id),
        cancellationToken
      )
    )?.ConfigurationDetail;

    if (configurationDetail is null)
    {
      return null;
    }

    return new CreateConfigurationResponse(configurationDetail);
  }
}
