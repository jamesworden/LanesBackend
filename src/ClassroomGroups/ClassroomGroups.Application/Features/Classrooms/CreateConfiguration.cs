using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateConfigurationRequest(string Label, Guid ClassroomId)
  : IRequest<CreateConfigurationResponse> { }

public record CreateConfigurationResponse(ConfigurationDetail CreatedConfigurationDetail) { }

public class CreateConfigurationRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService,
  IConfigurationService configurationService
) : IRequestHandler<CreateConfigurationRequest, CreateConfigurationResponse>
{
  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  readonly IConfigurationService _configurationService = configurationService;

  public async Task<CreateConfigurationResponse> Handle(
    CreateConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    var configuration = await _configurationService.CreateConfiguration(
      account.Id,
      request.ClassroomId,
      request.Label,
      cancellationToken
    );

    var configurationDetail =
      await _detailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        configuration.Id,
        cancellationToken
      ) ?? throw new Exception();

    return new CreateConfigurationResponse(configurationDetail);
  }
}
