using System.Security.Claims;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Behaviors;

public interface IOptionalUserAccount { }

public class AccountOptionalCache()
{
  public Account? Account { get; set; }

  public required ClaimsPrincipal User { get; set; }
}

public class AccountOptionalBehavior<TRequest, TResponse>(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor,
  AccountOptionalCache authBehaviorCache
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>, IOptionalUserAccount
{
  private readonly ClassroomGroupsContext _dbContext = dbContext;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
  private readonly AccountOptionalCache _authBehaviorCache = authBehaviorCache;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    var user =
      _httpContextAccessor.HttpContext?.User
      ?? throw new UnauthorizedAccessException("User must be authenticated.");

    _authBehaviorCache.User = user;
    _authBehaviorCache.Account = await GetAssociatedAccount(cancellationToken);

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
    return accountDTO is null || subscription is null
      ? null
      : accountDTO.ToAccount(subscription.ToSubscription());
  }
}
