using System.Security.Claims;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Application.Features.Classrooms.Responses;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class CreateConfigurationRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor,
  IMediator mediator
) : IRequestHandler<CreateConfigurationRequest, CreateConfigurationResponse?>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  readonly IMediator _mediator = mediator;

  public async Task<CreateConfigurationResponse?> Handle(
    CreateConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    if (_httpContextAccessor.HttpContext is null)
    {
      return null;
    }
    var googleNameIdentifier = _httpContextAccessor
      .HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
      ?.Value;
    if (googleNameIdentifier is null)
    {
      return null;
    }
    var accountDTO = await _dbContext.Accounts.FirstOrDefaultAsync(
      a => a.GoogleNameIdentifier == googleNameIdentifier,
      cancellationToken
    );
    if (accountDTO is null)
    {
      return null;
    }

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

    var configurationDetail = await _mediator.Send(
      new GetConfigurationDetailRequest(configuration.ClassroomId, configuration.Id),
      cancellationToken
    );

    if (configurationDetail is null)
    {
      return null;
    }

    return new CreateConfigurationResponse(configurationDetail);
  }
}
