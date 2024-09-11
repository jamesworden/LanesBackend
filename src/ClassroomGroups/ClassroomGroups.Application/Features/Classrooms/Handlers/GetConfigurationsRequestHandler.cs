using System.Security.Claims;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Application.Features.Classrooms.Responses;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class GetConfigurationsRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetConfigurationsRequest, GetConfigurationsResponse?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<GetConfigurationsResponse?> Handle(
    GetConfigurationsRequest request,
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
    var accountDTO = await _dbContext.Accounts.FirstOrDefaultAsync(
      a => a.GoogleNameIdentifier == googleNameIdentifier,
      cancellationToken
    );
    if (accountDTO is null)
    {
      return null;
    }

    var classroomIds = (
      await _dbContext
        .Classrooms.Where(c => c.AccountId == accountDTO.Id)
        .ToListAsync(cancellationToken)
    ).Select(c => c.Id);

    var configurations = (
      await _dbContext
        .Configurations.Where(c => classroomIds.Contains(c.ClassroomId))
        .ToListAsync(cancellationToken)
    )
      .Select(c => c.ToConfiguration())
      .ToList();

    return new GetConfigurationsResponse(configurations);
  }
}
