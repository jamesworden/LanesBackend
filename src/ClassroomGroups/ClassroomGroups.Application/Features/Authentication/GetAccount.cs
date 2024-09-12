using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;

namespace ClassroomGroups.Application.Features.Authentication;

public record GetAccountRequest() : IRequest<AccountView?> { }

public class GetAccountRequestHandler(AuthBehaviorCache authBehaviorCache)
  : IRequestHandler<GetAccountRequest, AccountView?>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<AccountView?> Handle(
    GetAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;
    await Task.CompletedTask;
    return account?.ToAccountView();
  }
}
