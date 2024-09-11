using System.Security.Claims;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
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
    if (_httpContextAccessor.HttpContext is null)
    {
      throw new Exception();
    }
    var googleNameIdentifier =
      (
        _httpContextAccessor
          .HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
          ?.Value
      ) ?? throw new Exception();

    var accountDTO =
      await _dbContext.Accounts.FirstOrDefaultAsync(
        a => a.GoogleNameIdentifier == googleNameIdentifier,
        cancellationToken
      ) ?? throw new Exception();

    _authBehaviorCache[AuthBehaviorItem.Account] = accountDTO.ToAccount();

    var response = await next();

    return response;
  }
}

public class AuthBehaviorCache() : Dictionary<string, object> { }

public static class AuthBehaviorItem
{
  public static readonly string Account = "Account";
}
