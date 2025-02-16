using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record GetClassroomDetailsRequest()
  : IRequest<GetClassroomDetailsResponse>,
    IRequiredUserAccount
{
  public EntityIds GetEntityIds() => new();
}

public record GetClassroomDetailsResponse(List<ClassroomDetail> ClassroomDetails) { }

public class GetClassroomDetailsRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache
) : IRequestHandler<GetClassroomDetailsRequest, GetClassroomDetailsResponse>
{
  public async Task<GetClassroomDetailsResponse> Handle(
    GetClassroomDetailsRequest request,
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
      ).Select(c => c.Id);

      var fieldDetails =
        (
          await dbContext
            .Fields.Where(f => classroomIds.Contains(f.ClassroomId))
            .Select(f => new FieldDetailDTO(f.Id, f.ClassroomId, f.Label, f.Type))
            .ToListAsync(cancellationToken)
        )
          .Select(f => f.ToFieldDetail())
          .ToList() ?? [];

      var classroomDetails =
        (
          await dbContext
            .Classrooms.Where(c => c.AccountKey == account.Key)
            .Select(c => new ClassroomDetailDTO(c.Id, c.AccountId, c.Label, c.Description))
            .ToListAsync(cancellationToken)
        )
          .Select(c => c.ToClassroomDetail(fieldDetails))
          .ToList() ?? [];

      transaction.Commit();

      return new GetClassroomDetailsResponse(classroomDetails);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
