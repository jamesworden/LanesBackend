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
  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    var account =
      await accountService.GetAssociatedAccountAsync(cancellationToken)
      ?? throw new UnauthorizedAccessException("User must be authenticated.");

    var user =
      httpContextAccessor.HttpContext?.User
      ?? throw new UnauthorizedAccessException("User must be authenticated.");

    authBehaviorCache.Account = account;
    authBehaviorCache.User = user;

    return await next();
  }
}
