using System.Security.Claims;
using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

    var freeSubscriptionDTO =
      await _dbContext.Subscriptions.FirstOrDefaultAsync(
        s => s.SubscriptionType == SubscriptionType.FREE,
        cancellationToken
      ) ?? throw new Exception();

    var accountId = Guid.NewGuid();

    var upsertedAccountDTO =
      (
        await _dbContext.AddAsync(
          new AccountDTO
          {
            Id = accountId,
            GoogleNameIdentifier = googleNameIdentifier,
            PrimaryEmail = primaryEmail,
            SubscriptionKey = freeSubscriptionDTO.Key,
            SubscriptionId = freeSubscriptionDTO.Id,
            SubscriptionDTO = freeSubscriptionDTO
          },
          cancellationToken
        )
      )?.Entity ?? throw new Exception();

    var subscriptionDTO =
      await _dbContext.Subscriptions.FirstOrDefaultAsync(
        (s) => s.Id == upsertedAccountDTO.SubscriptionId,
        cancellationToken
      ) ?? throw new Exception();

    await _dbContext.SaveChangesAsync(cancellationToken);

    var account =
      upsertedAccountDTO.ToAccount(subscriptionDTO.ToSubscription()).ToAccountView()
      ?? throw new Exception();

    return new UpsertAccountResponse(account);
  }
}
