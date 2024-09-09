using System.Security.Claims;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class GetClassroomsRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetClassroomsRequest, List<Classroom>?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<List<Classroom>?> Handle(
    GetClassroomsRequest request,
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

    var classrooms =
      (
        await _dbContext
          .Classrooms.Where(c => c.AccountKey == accountDTO.Key)
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.ToClassroom())
        .ToList() ?? [];

    return classrooms;
  }
}
