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
  string Label,
  string Description
) : IRequest<PatchConfigurationResponse>, IRequiredUserAccount { }

public record PatchConfigurationResponse(ConfigurationDetail PatchedConfigurationDetail) { }

public class PatchConfigurationRequestHandler(
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext dbContext
) : IRequestHandler<PatchConfigurationRequest, PatchConfigurationResponse>
{
  public async Task<PatchConfigurationResponse> Handle(
    PatchConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var classroomIds = (
        await dbContext
          .Classrooms.Where(c => c.AccountId == account.Id)
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.Id)
        .ToList();

      var configurationDTO =
        await dbContext
          .Configurations.Where(c =>
            c.ClassroomId == request.ClassroomId
            && c.Id == request.ConfigurationId
            && classroomIds.Contains(c.ClassroomId)
          )
          .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      configurationDTO.Description = request.Description.Trim();
      configurationDTO.Label = request.Label.Trim();

      var configurationEntity = dbContext.Configurations.Update(configurationDTO);
      await dbContext.SaveChangesAsync(cancellationToken);
      var configuration =
        configurationEntity.Entity?.ToConfiguration() ?? throw new InvalidOperationException();

      var configurationDetail =
        await detailService.GetConfigurationDetail(
          account.Id,
          request.ClassroomId,
          request.ConfigurationId,
          cancellationToken
        ) ?? throw new InvalidOperationException();

      transaction.Commit();

      return new PatchConfigurationResponse(configurationDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
