using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteConfigurationRequest(Guid ClassroomId, Guid ConfigurationId)
  : IRequest<DeleteConfigurationResponse>,
    IRequiredUserAccount { }

public record DeleteConfigurationResponse(Configuration DeletedConfiguration) { }

public class DeleteConfigurationRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache
) : IRequestHandler<DeleteConfigurationRequest, DeleteConfigurationResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AccountRequiredCache _authBehaviorCache = authBehaviorCache;

  public async Task<DeleteConfigurationResponse> Handle(
    DeleteConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var configurationDTO =
        _dbContext.Configurations.SingleOrDefault(c =>
          c.Id == request.ConfigurationId && c.ClassroomId == request.ClassroomId
        ) ?? throw new Exception();

      _dbContext.Configurations.Remove(configurationDTO);
      await _dbContext.SaveChangesAsync(cancellationToken);

      transaction.Commit();

      return new DeleteConfigurationResponse(configurationDTO.ToConfiguration());
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
