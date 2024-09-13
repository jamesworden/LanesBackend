using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms;

public record GetConfigurationDetailRequest(Guid ClassroomId, Guid ConfigurationId)
  : IRequest<GetConfigurationDetailResponse> { }

public record GetConfigurationDetailResponse(ConfigurationDetail ConfigurationDetail) { }

public class GetConfigurationDetailRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  IGetDetailService getConfigurationDetailService
) : IRequestHandler<GetConfigurationDetailRequest, GetConfigurationDetailResponse>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly IGetDetailService _getConfigurationDetailService = getConfigurationDetailService;

  public async Task<GetConfigurationDetailResponse> Handle(
    GetConfigurationDetailRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    var configurationDetail =
      await _getConfigurationDetailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      ) ?? throw new Exception();

    return new GetConfigurationDetailResponse(configurationDetail);
  }
}
