using System.Security.Claims;
using ClassroomGroups.Application.Features.Accounts.Requests;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Accounts.Handlers;

public class GetAccountRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetAccountRequest, Account?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<Account?> Handle(GetAccountRequest request, CancellationToken cancellationToken)
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
    return accountDTO?.ToAccount();
  }
}
