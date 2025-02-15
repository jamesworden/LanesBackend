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
  ClassroomGroupsContext dbContext
) : IRequestHandler<PatchClassroomRequest, PatchClassroomResponse>
{
  public async Task<PatchClassroomResponse> Handle(
    PatchClassroomRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var classroomIds = (
        await dbContext
          .Classrooms.Where(c => c.AccountId == account.Id)
          .ToListAsync(cancellationToken)
      )
        .Select(c => c.Id)
        .ToList();

      var classroomDTO =
        await dbContext
          .Classrooms.Where(c => c.Id == request.ClassroomId && classroomIds.Contains(c.Id))
          .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      classroomDTO.Description = request.Description.Trim();
      classroomDTO.Label = request.Label.Trim();

      var configurationEntity = dbContext.Classrooms.Update(classroomDTO);
      await dbContext.SaveChangesAsync(cancellationToken);
      var classroom =
        configurationEntity.Entity?.ToClassroom() ?? throw new InvalidOperationException();

      var fieldDetails =
        (
          await dbContext
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
