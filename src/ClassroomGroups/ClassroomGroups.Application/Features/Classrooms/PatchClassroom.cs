using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record PatchClassroomRequest(Guid ClassroomId, Classroom Classroom)
  : IRequest<PatchClassroomResponse> { }

public record PatchClassroomRequestBody(Classroom Classroom) { }

public record PatchClassroomResponse(ClassroomDetail PatchedClassroomDetail) { }

public class PatchClassroomRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  ClassroomGroupsContext classroomGroupsContext
) : IRequestHandler<PatchClassroomRequest, PatchClassroomResponse>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly ClassroomGroupsContext _dbContext = classroomGroupsContext;

  public async Task<PatchClassroomResponse> Handle(
    PatchClassroomRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    var classroomIds = (
      await _dbContext
        .Classrooms.Where(c => c.AccountId == account.Id)
        .ToListAsync(cancellationToken)
    )
      .Select(c => c.Id)
      .ToList();

    var classroomDTO =
      await _dbContext
        .Classrooms.Where(c => c.Id == request.ClassroomId && classroomIds.Contains(c.Id))
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    classroomDTO.Description = request.Classroom.Description;
    classroomDTO.Label = request.Classroom.Label;

    var configurationEntity = _dbContext.Classrooms.Update(classroomDTO);
    await _dbContext.SaveChangesAsync(cancellationToken);
    var classroom = configurationEntity.Entity?.ToClassroom() ?? throw new Exception();

    var fieldDetails =
      (
        await _dbContext
          .Fields.Where(f => classroomIds.Contains(f.ClassroomId))
          .Select(f => new FieldDetailDTO(f.Id, f.ClassroomId, f.Label, f.Type))
          .ToListAsync(cancellationToken)
      )
        .Select(f => f.ToFieldDetail())
        .ToList() ?? [];

    var classroomDetail = new ClassroomDetail(
      classroom.Id,
      classroom.AccountId,
      classroom.Label,
      classroom.Description,
      fieldDetails
    );

    return new PatchClassroomResponse(classroomDetail);
  }
}
