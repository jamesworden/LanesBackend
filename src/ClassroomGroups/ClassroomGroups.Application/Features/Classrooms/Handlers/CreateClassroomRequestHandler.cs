using System.Security.Claims;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Application.Features.Classrooms.Responses;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class CreateClassroomRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor,
  IMediator mediator
) : IRequestHandler<CreateClassroomRequest, CreateClassroomResponse?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  IMediator _mediator = mediator;

  public async Task<CreateClassroomResponse?> Handle(
    CreateClassroomRequest request,
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

    var classroomDTO = new ClassroomDTO()
    {
      Id = Guid.NewGuid(),
      Label = request.Label,
      Description = request.Description,
      AccountKey = accountDTO.Key,
      AccountId = accountDTO.Id
    };
    var classroomEntity = await _dbContext.Classrooms.AddAsync(classroomDTO, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
    var classroom = classroomEntity.Entity?.ToClassroom();
    if (classroom is null)
    {
      return null;
    }

    var fieldDetails =
      (
        await _dbContext
          .Fields.Where(f => f.ClassroomId == classroom.Id)
          .Select(f => new FieldDetailDTO(f.Id, f.ClassroomId, f.Label, f.Type))
          .ToListAsync(cancellationToken)
      )
        .Select(f => f.ToFieldDetail())
        .ToList() ?? [];

    var createdClassroomDetail = classroom.ToClassroomDetail(fieldDetails);

    return new CreateClassroomResponse(createdClassroomDetail);
  }
}
