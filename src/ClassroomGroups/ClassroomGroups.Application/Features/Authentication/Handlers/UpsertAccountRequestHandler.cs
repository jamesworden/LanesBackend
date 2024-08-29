using System.Security.Claims;
using ClassroomGroups.Application.Features.Authentication.Requests;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Authentication.Handlers;

public class UpsertAccountRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<UpsertAccountRequest, Account?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<Account?> Handle(
    UpsertAccountRequest request,
    CancellationToken cancellationToken
  )
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
    var primaryEmail = _httpContextAccessor
      .HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)
      ?.Value;
    if (primaryEmail is null)
    {
      return null;
    }
    var existingAccountDTO = await _dbContext.Accounts.FirstOrDefaultAsync(
      a => a.GoogleNameIdentifier == googleNameIdentifier,
      cancellationToken
    );
    if (existingAccountDTO is not null)
    {
      return existingAccountDTO.ToAccount();
    }
    var accountDTO = await _dbContext.Accounts.AddAsync(
      new()
      {
        Id = Guid.NewGuid(),
        GoogleNameIdentifier = googleNameIdentifier,
        PrimaryEmail = primaryEmail
      },
      cancellationToken
    );
    await _dbContext.SaveChangesAsync(cancellationToken);
    return accountDTO.Entity?.ToAccount();
  }
}
