using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateConfigurationRequest(string Label, Guid ClassroomId)
  : IRequest<CreateConfigurationResponse>,
    IRequiredUserAccount
{
  public EntityIds GetEntityIds() => new() { ClassroomIds = [ClassroomId] };
}

public record CreateConfigurationResponse(ConfigurationDetail CreatedConfigurationDetail) { }

public class CreateConfigurationRequestHandler(
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService,
  IConfigurationService configurationService,
  ClassroomGroupsContext dbContext
) : IRequestHandler<CreateConfigurationRequest, CreateConfigurationResponse>
{
  public async Task<CreateConfigurationResponse> Handle(
    CreateConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    var existingConfigurationDTOs = await dbContext
      .Configurations.Where(c => c.ClassroomId == request.ClassroomId)
      .ToListAsync(cancellationToken);

    if (existingConfigurationDTOs.Count >= account.Subscription.MaxConfigurationsPerClassroom)
    {
      throw new Exception();
    }

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var configuration = await configurationService.CreateConfiguration(
        account.Id,
        request.ClassroomId,
        request.Label,
        cancellationToken
      );

      var configurationDetail =
        await detailService.GetConfigurationDetail(
          account.Id,
          request.ClassroomId,
          configuration.Id,
          cancellationToken
        ) ?? throw new InvalidOperationException();

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
