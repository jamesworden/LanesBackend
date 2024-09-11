using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Application.Features.Classrooms.Responses;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class CreateClassroomRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache
) : IRequestHandler<CreateClassroomRequest, CreateClassroomResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<CreateClassroomResponse> Handle(
    CreateClassroomRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = (Account)_authBehaviorCache[AuthBehaviorItem.Account];

    var classroomDTO = new ClassroomDTO()
    {
      Id = Guid.NewGuid(),
      Label = request.Label,
      Description = request.Description,
      AccountKey = account.Key,
      AccountId = account.Id
    };
    var classroomEntity = await _dbContext.Classrooms.AddAsync(classroomDTO, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
    var classroom = (classroomEntity.Entity?.ToClassroom()) ?? throw new Exception();

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
