using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record PatchClassroomRequest(Guid ClassroomId, string Label, string Description)
  : IRequest<PatchClassroomResponse>,
    IRequiredUserAccount { }

public record PatchClassroomResponse(ClassroomDetail PatchedClassroomDetail) { }

public class PatchClassroomRequestHandler(
  AccountRequiredCache authBehaviorCache,
  ClassroomGroupsContext classroomGroupsContext
) : IRequestHandler<PatchClassroomRequest, PatchClassroomResponse>
{
  readonly AccountRequiredCache _authBehaviorCache = authBehaviorCache;

  readonly ClassroomGroupsContext _dbContext = classroomGroupsContext;

  public async Task<PatchClassroomResponse> Handle(
    PatchClassroomRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
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

      classroomDTO.Description = request.Description.Trim();
      classroomDTO.Label = request.Label.Trim();

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

      transaction.Commit();

      return new PatchClassroomResponse(classroomDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
