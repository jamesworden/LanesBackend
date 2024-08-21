using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClassroomGroups.Application.Behaviors;

/// <summary>
/// See https://www.jimmybogard.com/sharing-context-in-mediatr-pipelines/
/// </summary>
public class AuthenticationBehavior<TRequest, TResponse>(
  IHttpContextAccessor httpContextAccessor,
  ItemsCache itemsCache
) : IPipelineBehavior<TRequest, TResponse>
  where TRequest : ContextualRequest<TResponse>
{
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
  private readonly ItemsCache _itemsCache = itemsCache;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken
  )
  {
    _itemsCache["User"] = _httpContextAccessor.HttpContext.User;

    return await next();
  }
}

public class ItemsCache : Dictionary<string, object> { }

public abstract class ContextualRequest<TResponse> : IRequest<TResponse>
{
  public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
}
