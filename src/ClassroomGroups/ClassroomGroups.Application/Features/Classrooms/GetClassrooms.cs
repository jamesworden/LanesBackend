using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record GetClassroomsRequest(Guid ConfigurationId)
  : IRequest<GetClassroomsResponse>,
    IRequiredUserAccount { }

public record GetClassroomsResponse(List<Classroom> Classrooms) { }

public class GetClassroomsRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache
) : IRequestHandler<GetClassroomsRequest, GetClassroomsResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AccountRequiredCache _authBehaviorCache = authBehaviorCache;

  public async Task<GetClassroomsResponse> Handle(
    GetClassroomsRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;

    var classrooms =
      (
        await _dbContext
          .Classrooms.Where(c => c.AccountKey == account.Key)
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.ToClassroom())
        .ToList() ?? [];

    return new GetClassroomsResponse(classrooms);
  }
}
