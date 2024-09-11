using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Authentication.Requests;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;

namespace ClassroomGroups.Application.Features.Authentication.Handlers;

public class GetAccountRequestHandler(AuthBehaviorCache authBehaviorCache)
  : IRequestHandler<GetAccountRequest, AccountView>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<AccountView> Handle(
    GetAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = (Account)_authBehaviorCache["Account"];
    await Task.CompletedTask;
    return account.ToAccountView();
  }
}
