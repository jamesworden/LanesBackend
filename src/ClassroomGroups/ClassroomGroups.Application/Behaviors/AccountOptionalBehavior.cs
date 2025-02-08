using System.Security.Claims;
using ClassroomGroups.Application.Behaviors.Shared;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClassroomGroups.Application.Behaviors;

public interface IOptionalUserAccount { }

public class AccountOptionalCache()
{
  public Account? Account { get; set; }
  public required ClaimsPrincipal User { get; set; }
}

public class AccountOptionalBehavior<TRequest, TResponse>(
  IAccountService accountService,
  AccountOptionalCache authBehaviorCache,
  IHttpContextAccessor httpContextAccessor
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>, IOptionalUserAccount
{
  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    var user =
      httpContextAccessor.HttpContext?.User
      ?? throw new UnauthorizedAccessException("User must be authenticated.");

    authBehaviorCache.User = user;
    authBehaviorCache.Account = await accountService.GetAssociatedAccountAsync(cancellationToken);

    return await next();
  }
}
