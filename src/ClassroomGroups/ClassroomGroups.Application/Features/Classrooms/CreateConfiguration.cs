using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateConfigurationRequest(string Label, Guid ClassroomId)
  : IRequest<CreateConfigurationResponse>,
    IRequiredUserAccount { }

public record CreateConfigurationResponse(ConfigurationDetail CreatedConfigurationDetail) { }

public class CreateConfigurationRequestHandler(
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService,
  IConfigurationService configurationService,
  ClassroomGroupsContext dbContext
) : IRequestHandler<CreateConfigurationRequest, CreateConfigurationResponse>
{
  readonly AccountRequiredCache authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  readonly IConfigurationService _configurationService = configurationService;

  readonly ClassroomGroupsContext _dbContext = dbContext;

  public async Task<CreateConfigurationResponse> Handle(
    CreateConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    var existingConfigurationDTOs = await _dbContext
      .Configurations.Where(c => c.ClassroomId == request.ClassroomId)
      .ToListAsync(cancellationToken);

    if (existingConfigurationDTOs.Count >= account.Subscription.MaxConfigurationsPerClassroom)
    {
      throw new Exception();
    }

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
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

      transaction.Commit();

      return new CreateConfigurationResponse(configurationDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
