using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteClassroomRequest(Guid ClassroomId) : IRequest<DeleteClassroomResponse> { }

public record DeleteClassroomResponse(Classroom DeletedClassroom) { }

public class DeleteClassroomRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache
) : IRequestHandler<DeleteClassroomRequest, DeleteClassroomResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  public async Task<DeleteClassroomResponse> Handle(
    DeleteClassroomRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var classroom =
        _dbContext.Classrooms.SingleOrDefault(c =>
          c.Id == request.ClassroomId && c.AccountId == account.Id
        ) ?? throw new Exception();

      _dbContext.Classrooms.Remove(classroom);
      await _dbContext.SaveChangesAsync(cancellationToken);

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
