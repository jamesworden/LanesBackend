using System.Security.Claims;
using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;

namespace ClassroomGroups.Application.Features.Authentication;

public record UpsertAccountRequest() : IRequest<UpsertAccountResponse> { }

public record UpsertAccountResponse(AccountView Account) { }

public class UpsertAccountRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache
) : IRequestHandler<UpsertAccountRequest, UpsertAccountResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<UpsertAccountResponse> Handle(
    UpsertAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    var existingAccount = _authBehaviorCache.Account;
    if (existingAccount is not null)
    {
      return new UpsertAccountResponse(existingAccount.ToAccountView());
    }
    var user = _authBehaviorCache.User ?? throw new Exception();

    var primaryEmail =
      (user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value)
      ?? throw new Exception();

    var googleNameIdentifier =
      (user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value)
      ?? throw new Exception();

    var accountDTO = await _dbContext.Accounts.AddAsync(
      new()
      {
        Id = Guid.NewGuid(),
        GoogleNameIdentifier = googleNameIdentifier,
        PrimaryEmail = primaryEmail
      },
      cancellationToken
    );
    await _dbContext.SaveChangesAsync(cancellationToken);
    var account = accountDTO.Entity.ToAccount().ToAccountView() ?? throw new Exception();
    return new UpsertAccountResponse(account);
  }
}
