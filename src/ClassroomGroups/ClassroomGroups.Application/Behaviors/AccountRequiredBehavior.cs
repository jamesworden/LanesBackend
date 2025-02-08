using System.Security.Claims;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Behaviors;

public interface IRequiredUserAccount { }

public class AccountRequiredCache()
{
  public required Account Account { get; set; }

  public required ClaimsPrincipal User { get; set; }
}

public class AccountRequiredBehavior<TRequest, TResponse>(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor,
  AccountRequiredCache authBehaviorCache
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>, IRequiredUserAccount
{
  private readonly ClassroomGroupsContext _dbContext = dbContext;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
  private readonly AccountRequiredCache _authBehaviorCache = authBehaviorCache;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    var account =
      await GetAssociatedAccount(cancellationToken)
      ?? throw new UnauthorizedAccessException("User must be authenticated.");

    var user =
      _httpContextAccessor.HttpContext?.User
      ?? throw new UnauthorizedAccessException("User must be authenticated.");

    _authBehaviorCache.Account = account;
    _authBehaviorCache.User = user;

    return await next();
  }

  private async Task<Account?> GetAssociatedAccount(CancellationToken cancellationToken)
  {
    var user = _httpContextAccessor.HttpContext?.User;
    if (user is null)
      return null;

    var googleNameIdentifier = user
      .Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
      ?.Value;
    if (string.IsNullOrEmpty(googleNameIdentifier))
      return null;

    var accountDTO = await _dbContext.Accounts.FirstOrDefaultAsync(
      a => a.GoogleNameIdentifier == googleNameIdentifier,
      cancellationToken
    );
    if (accountDTO is null)
    {
      return null;
    }
    var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(
      s => s.Id == accountDTO.SubscriptionId,
      cancellationToken
    );
    return subscription is null ? null : accountDTO.ToAccount(subscription.ToSubscription());
  }
}
