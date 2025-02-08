using System.Security.Claims;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Behaviors.Shared;

public interface IAccountService
{
  Task<Account?> GetAssociatedAccountAsync(CancellationToken cancellationToken);
}

public class AccountService(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IAccountService
{
  public async Task<Account?> GetAssociatedAccountAsync(CancellationToken cancellationToken)
  {
    var user = httpContextAccessor.HttpContext?.User;
    if (user is null)
      return null;

    var googleNameIdentifier = user
      .Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
      ?.Value;

    if (string.IsNullOrEmpty(googleNameIdentifier))
      return null;

    var accountDTO = await dbContext.Accounts.FirstOrDefaultAsync(
      a => a.GoogleNameIdentifier == googleNameIdentifier,
      cancellationToken
    );

    if (accountDTO is null)
      return null;

    var subscription = await dbContext.Subscriptions.FirstOrDefaultAsync(
      s => s.Id == accountDTO.SubscriptionId,
      cancellationToken
    );

    return subscription is null ? null : accountDTO.ToAccount(subscription.ToSubscription());
  }
}
