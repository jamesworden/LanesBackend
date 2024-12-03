using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Authentication;

public record GetAccountRequest() : IRequest<GetAccountResponse> { }

public record GetAccountResponse(AccountView? Account) { }

public class GetAccountRequestHandler(AuthBehaviorCache authBehaviorCache)
  : IRequestHandler<GetAccountRequest, GetAccountResponse>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<GetAccountResponse> Handle(
    GetAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;
    await Task.CompletedTask;
    return new GetAccountResponse(account?.ToAccountView());
  }
}
