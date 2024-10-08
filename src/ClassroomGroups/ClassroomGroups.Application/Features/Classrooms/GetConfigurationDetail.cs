using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms;

public record GetConfigurationDetailRequest(Guid ClassroomId, Guid ConfigurationId)
  : IRequest<GetConfigurationDetailResponse> { }

public record GetConfigurationDetailResponse(ConfigurationDetail ConfigurationDetail) { }

public class GetConfigurationDetailRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext dbContext
) : IRequestHandler<GetConfigurationDetailRequest, GetConfigurationDetailResponse>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  readonly ClassroomGroupsContext _dbContext = dbContext;

  public async Task<GetConfigurationDetailResponse> Handle(
    GetConfigurationDetailRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var configurationDetail =
        await _detailService.GetConfigurationDetail(
          account.Id,
          request.ClassroomId,
          request.ConfigurationId,
          cancellationToken
        ) ?? throw new Exception();

      transaction.Commit();

      return new GetConfigurationDetailResponse(configurationDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
