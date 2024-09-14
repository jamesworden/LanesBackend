using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteConfigurationRequest(Guid ClassroomId, Guid ConfigurationId)
  : IRequest<DeleteConfigurationResponse> { }

public record DeleteConfigurationResponse(Configuration DeletedConfiguration) { }

public class DeleteConfigurationRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache
) : IRequestHandler<DeleteConfigurationRequest, DeleteConfigurationResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  public async Task<DeleteConfigurationResponse> Handle(
    DeleteConfigurationRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    var configurationDTO =
      _dbContext.Configurations.SingleOrDefault(c =>
        c.Id == request.ConfigurationId && c.ClassroomId == request.ClassroomId
      ) ?? throw new Exception();

    _dbContext.Configurations.Remove(configurationDTO);
    await _dbContext.SaveChangesAsync(cancellationToken);

    return new DeleteConfigurationResponse(configurationDTO.ToConfiguration());
  }
}
