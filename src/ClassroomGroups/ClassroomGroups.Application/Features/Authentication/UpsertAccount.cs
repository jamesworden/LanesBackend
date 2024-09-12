using System.Security.Claims;
using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;

namespace ClassroomGroups.Application.Features.Authentication;

public record UpsertAccountRequest() : IRequest<AccountView> { }

public class UpsertAccountRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache
) : IRequestHandler<UpsertAccountRequest, AccountView?>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<AccountView?> Handle(
    UpsertAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;
    if (account is not null)
    {
      return account.ToAccountView();
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
    return accountDTO.Entity?.ToAccount().ToAccountView();
  }
}
