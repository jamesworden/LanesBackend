using System.Security.Claims;
using ClassroomGroups.Application.Features.Accounts.Responses;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Application.Features.Classrooms.Responses;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class DeleteClassroomRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<DeleteClassroomRequest, DeleteClassroomResponse?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<DeleteClassroomResponse?> Handle(
    DeleteClassroomRequest request,
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

    var classroom = _dbContext.Classrooms.SingleOrDefault(c =>
      c.Id == request.ClassroomId && c.AccountId == accountDTO.Id
    );
    if (classroom is null)
    {
      return null;
    }

    _dbContext.Classrooms.Remove(classroom);
    await _dbContext.SaveChangesAsync(cancellationToken);

    return new DeleteClassroomResponse(classroom.ToClassroom());
  }
}
