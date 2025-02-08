using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record PatchFieldRequest(Guid ClassroomId, Guid FieldId, string Label)
  : IRequest<PatchFieldResponse>,
    IRequiredUserAccount { }

public record PatchFieldResponse(FieldDetail UpdatedFieldDetail) { }

public class PatchFieldRequestHandler(
  AccountRequiredCache authBehaviorCache,
  ClassroomGroupsContext dbContext
) : IRequestHandler<PatchFieldRequest, PatchFieldResponse>
{
  public async Task<PatchFieldResponse> Handle(
    PatchFieldRequest request,
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

      var fieldDTO =
        await dbContext
          .Fields.Where(f =>
            f.Id == request.FieldId
            && f.ClassroomId == request.ClassroomId
            && classroomIds.Contains(f.ClassroomId)
          )
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      fieldDTO.Label = request.Label.Trim();

      var fieldEntity = dbContext.Fields.Update(fieldDTO);
      await dbContext.SaveChangesAsync(cancellationToken);

      var fieldDetail = fieldEntity.Entity?.ToField().ToFieldDetail() ?? throw new Exception();

      transaction.Commit();

      return new PatchFieldResponse(fieldDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
