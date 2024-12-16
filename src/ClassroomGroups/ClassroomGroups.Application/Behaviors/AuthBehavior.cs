using System.Security.Claims;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Behaviors;

public class AuthBehavior<TRequest, TResponse>(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor,
  AuthBehaviorCache authBehaviorCache
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
{
  private readonly ClassroomGroupsContext _dbContext = dbContext;

  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  private readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    _authBehaviorCache.Account = await GetAssociatedAccount(cancellationToken);
    _authBehaviorCache.User = _httpContextAccessor.HttpContext.User;
    var response = await next();
    return response;
  }

  private async Task<Account?> GetAssociatedAccount(CancellationToken cancellationToken)
  {
    if (_httpContextAccessor.HttpContext is null)
    {
      return null;
    }
    var googleNameIdentifier = _httpContextAccessor
      .HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
      ?.Value;
    if (googleNameIdentifier is null)
    {
      return null;
    }
    var accountDTO = await _dbContext.Accounts.FirstOrDefaultAsync(
      a => a.GoogleNameIdentifier == googleNameIdentifier,
      cancellationToken
    );
    if (accountDTO is null)
    {
      return null;
    }
    var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(
      (s) => s.Id == accountDTO.SubscriptionId,
      cancellationToken
    );
    if (subscription is null)
    {
      return null;
    }
    return accountDTO.ToAccount(subscription.ToSubscription());
  }
}

public class AuthBehaviorCache()
{
  public Account? Account { get; set; }

  public ClaimsPrincipal? User { get; set; }
}
