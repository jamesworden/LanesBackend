using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record PatchConfigurationRequest(
  Guid ClassroomId,
  Guid ConfigurationId,
  Configuration Configuration
) : IRequest<PatchConfigurationResponse> { }

public record PatchConfigurationResponse(ConfigurationDetail PatchedConfigurationDetail) { }

public class PatchConfigurationRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext classroomGroupsContext
) : IRequestHandler<PatchConfigurationRequest, PatchConfigurationResponse>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  readonly ClassroomGroupsContext _dbContext = classroomGroupsContext;

  public async Task<PatchConfigurationResponse> Handle(
    PatchConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    var classroomIds = (
      await _dbContext
        .Classrooms.Where(c => c.AccountId == account.Id)
        .ToListAsync(cancellationToken)
    )
      .Select(c => c.Id)
      .ToList();

    var configurationDTO =
      await _dbContext
        .Configurations.Where(c =>
          c.ClassroomId == request.ClassroomId
          && c.Id == request.ConfigurationId
          && classroomIds.Contains(c.ClassroomId)
        )
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    configurationDTO.Description = request.Configuration.Description;
    configurationDTO.Label = request.Configuration.Label;

    var configurationEntity = _dbContext.Configurations.Update(configurationDTO);
    await _dbContext.SaveChangesAsync(cancellationToken);
    var configuration = configurationEntity.Entity?.ToConfiguration() ?? throw new Exception();

    var configurationDetail =
      await _detailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      ) ?? throw new Exception();

    return new PatchConfigurationResponse(configurationDetail);
  }
}
