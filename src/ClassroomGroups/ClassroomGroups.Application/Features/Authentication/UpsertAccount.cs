using System.Security.Claims;
using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Authentication;

public record UpsertAccountRequest() : IRequest<UpsertAccountResponse>, IOptionalUserAccount { }

public record UpsertAccountResponse(AccountView Account) { }

public class UpsertAccountRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountOptionalCache optionalAccountCache
) : IRequestHandler<UpsertAccountRequest, UpsertAccountResponse>
{
  public async Task<UpsertAccountResponse> Handle(
    UpsertAccountRequest request,
    CancellationToken cancellationToken
  )
  {
    var existingAccount = optionalAccountCache.Account;
    if (existingAccount is not null)
    {
      return new UpsertAccountResponse(existingAccount.ToAccountView());
    }
    var user = optionalAccountCache.User;

    var primaryEmail =
      (user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value)
      ?? throw new Exception();

    var googleNameIdentifier =
      (user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value)
      ?? throw new Exception();

    var freeSubscriptionDTO =
      await dbContext.Subscriptions.FirstOrDefaultAsync(
        s => s.SubscriptionType == SubscriptionType.FREE,
        cancellationToken
      ) ?? throw new Exception();

    var accountId = Guid.NewGuid();

    var upsertedAccountDTO =
      (
        await dbContext.AddAsync(
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
      await dbContext.Subscriptions.FirstOrDefaultAsync(
        (s) => s.Id == upsertedAccountDTO.SubscriptionId,
        cancellationToken
      ) ?? throw new Exception();

    await dbContext.SaveChangesAsync(cancellationToken);

    var account =
      upsertedAccountDTO.ToAccount(subscriptionDTO.ToSubscription()).ToAccountView()
      ?? throw new Exception();

    return new UpsertAccountResponse(account);
  }
}
