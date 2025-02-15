using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteClassroomRequest(Guid ClassroomId)
  : IRequest<DeleteClassroomResponse>,
    IRequiredUserAccount { }

public record DeleteClassroomResponse(Classroom DeletedClassroom) { }

public class DeleteClassroomRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache
) : IRequestHandler<DeleteClassroomRequest, DeleteClassroomResponse>
{
  public async Task<DeleteClassroomResponse> Handle(
    DeleteClassroomRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var classroom =
        dbContext.Classrooms.SingleOrDefault(c =>
          c.Id == request.ClassroomId && c.AccountId == account.Id
        ) ?? throw new InvalidOperationException();

      dbContext.Classrooms.Remove(classroom);
      await dbContext.SaveChangesAsync(cancellationToken);

      transaction.Commit();

      return new DeleteClassroomResponse(classroom.ToClassroom());
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
