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

public class GetClassroomDetailsRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetClassroomDetailRequest, GetClassroomDetailsResponse?>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<GetClassroomDetailsResponse?> Handle(
    GetClassroomDetailRequest request,
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

    var fieldDetails =
      (
        await _dbContext
          .Fields.Where(f => classroomIds.Contains(f.ClassroomId))
          .Select(f => new FieldDetailDTO(f.Id, f.ClassroomId, f.Label, f.Type))
          .ToListAsync(cancellationToken)
      )
        .Select(f => f.ToFieldDetail())
        .ToList() ?? [];

    var classroomDetails =
      (
        await _dbContext
          .Classrooms.Where(c => c.AccountKey == accountDTO.Key)
          .Select(c => new ClassroomDetailDTO(c.Id, c.AccountId, c.Label, c.Description))
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.ToClassroomDetail(fieldDetails))
        .ToList() ?? [];

    return new GetClassroomDetailsResponse(classroomDetails);
  }
}
