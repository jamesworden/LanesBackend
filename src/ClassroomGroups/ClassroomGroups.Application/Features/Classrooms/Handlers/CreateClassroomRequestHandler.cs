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
      Description = "",
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

    var createConfigurationRequest = new CreateConfigurationRequest(request.Label, classroom.Id);

    var configurationRes = await _mediator.Send(createConfigurationRequest);

    if (configurationRes is null)
    {
      return null;
    }

    return new CreateClassroomResponse(classroom, configurationRes.Configuration);
  }
}
