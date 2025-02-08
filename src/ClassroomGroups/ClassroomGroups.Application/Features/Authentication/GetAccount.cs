using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Authentication;

public record GetAccountRequest() : IRequest<GetAccountResponse>, IOptionalUserAccount { }

public record GetAccountResponse(AccountView? Account) { }

public class GetAccountRequestHandler(AccountOptionalCache accountOptionalCache)
  : IRequestHandler<GetAccountRequest, GetAccountResponse>
{
  public async Task<GetAccountResponse> Handle(
    GetAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = accountOptionalCache.Account;
    await Task.CompletedTask;
    return new GetAccountResponse(account?.ToAccountView());
  }
}
