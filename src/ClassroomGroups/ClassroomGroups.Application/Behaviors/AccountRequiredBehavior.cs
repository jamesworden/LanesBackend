using System.Security.Claims;
using ClassroomGroups.Application.Behaviors.Shared;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClassroomGroups.Application.Behaviors;

public interface IRequiredUserAccount { }

public class AccountRequiredCache()
{
  public required Account Account { get; set; }
  public required ClaimsPrincipal User { get; set; }
}

public class AccountRequiredBehavior<TRequest, TResponse>(
  IAccountService accountService,
  AccountRequiredCache authBehaviorCache,
  IHttpContextAccessor httpContextAccessor
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>, IRequiredUserAccount
{
  private readonly IAccountService _accountService = accountService;
  private readonly AccountRequiredCache _authBehaviorCache = authBehaviorCache;
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    var account =
      await _accountService.GetAssociatedAccountAsync(cancellationToken)
      ?? throw new UnauthorizedAccessException("User must be authenticated.");

    var user =
      _httpContextAccessor.HttpContext?.User
      ?? throw new UnauthorizedAccessException("User must be authenticated.");

    _authBehaviorCache.Account = account;
    _authBehaviorCache.User = user;

    return await next();
  }
}
