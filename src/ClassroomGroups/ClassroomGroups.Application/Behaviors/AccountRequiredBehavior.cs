using System.Security.Claims;
using ClassroomGroups.Application.Behaviors.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Authentication.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Behaviors;

public interface IRequiredUserAccount
{
  /// <returns>All entity IDs relevant to the current request, used for validating user access.</returns>
  public EntityIds GetEntityIds();
}

public class AccountRequiredCache()
{
  public required Account Account { get; set; }

  public required ClaimsPrincipal User { get; set; }
}

public record EntityIds
{
  public IReadOnlyCollection<Guid> ClassroomIds { get; init; } = [];

  public IReadOnlyCollection<Guid> StudentIds { get; init; } = [];

  public IReadOnlyCollection<Guid> ConfigurationIds { get; init; } = [];

  public IReadOnlyCollection<Guid> ColumnIds { get; init; } = [];

  public IReadOnlyCollection<Guid> FieldIds { get; init; } = [];

  public IReadOnlyCollection<Guid> GroupIds { get; init; } = [];
}

public class AccountRequiredBehavior<TRequest, TResponse>(
  IAccountService accountService,
  AccountRequiredCache authBehaviorCache,
  IHttpContextAccessor httpContextAccessor,
  ClassroomGroupsContext dbContext
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

    var entityIds = request.GetEntityIds();
    var authorized = await ValidateEntityOwnership(entityIds);
    if (!authorized)
    {
      throw new UnauthorizedAccessException("You are not authoized to handle the specified data.");
    }

    return await next();
  }

  public async Task<bool> ValidateEntityOwnership(EntityIds entityIds)
  {
    var accountId = authBehaviorCache.Account.Id;
    var validClassroomIds = new List<Guid>();
    var validConfigurationIds = new List<Guid>();
    var validColumnIds = new List<Guid>();
    var validFieldIds = new List<Guid>();
    var validGroupIds = new List<Guid>();

    if (entityIds.ClassroomIds.Any())
    {
      var classrooms = await dbContext
        .Classrooms.Where(c => entityIds.ClassroomIds.Contains(c.Id) && c.AccountId == accountId)
        .ToListAsync();

      if (classrooms.Count != entityIds.ClassroomIds.Count)
      {
        return false;
      }

      validClassroomIds = classrooms.Select(c => c.Id).ToList();
    }

    if (entityIds.FieldIds.Any())
    {
      var fields = await dbContext
        .Fields.Where(f =>
          entityIds.FieldIds.Contains(f.Id) && validClassroomIds.Contains(f.ClassroomId)
        )
        .ToListAsync();

      if (fields.Count != entityIds.FieldIds.Count)
      {
        return false;
      }

      validFieldIds = fields.Select(f => f.Id).ToList();
    }

    if (entityIds.ConfigurationIds.Any())
    {
      var configurations = await dbContext
        .Configurations.Where(c =>
          entityIds.ConfigurationIds.Contains(c.Id) && validClassroomIds.Contains(c.ClassroomId)
        )
        .ToListAsync();

      if (configurations.Count != entityIds.ConfigurationIds.Count)
      {
        return false;
      }

      validConfigurationIds = configurations.Select(c => c.Id).ToList();
    }

    if (entityIds.GroupIds.Any())
    {
      var groups = await dbContext
        .Groups.Where(g =>
          entityIds.GroupIds.Contains(g.Id) && validConfigurationIds.Contains(g.ConfigurationId)
        )
        .ToListAsync();

      if (groups.Count != entityIds.GroupIds.Count)
      {
        return false;
      }

      validGroupIds = groups.Select(g => g.Id).ToList();
    }

    if (entityIds.ColumnIds.Any())
    {
      var columns = await dbContext
        .Columns.Where(c =>
          entityIds.ColumnIds.Contains(c.Id) && validConfigurationIds.Contains(c.ConfigurationId)
        )
        .ToListAsync();

      if (columns.Count != entityIds.ColumnIds.Count)
      {
        return false;
      }

      validColumnIds = columns.Select(c => c.Id).ToList();
    }

    if (entityIds.StudentIds.Any())
    {
      var students = await dbContext
        .Students.Where(s =>
          entityIds.StudentIds.Contains(s.Id) && validClassroomIds.Contains(s.ClassroomId)
        )
        .ToListAsync();

      if (students.Count != entityIds.StudentIds.Count)
      {
        return false;
      }
    }

    return true;
  }
}
