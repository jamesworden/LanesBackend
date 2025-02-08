using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteStudentRequest(Guid ClassroomId, Guid StudentId)
  : IRequest<DeleteStudentResponse>,
    IRequiredUserAccount { };

public record DeleteStudentResponse(Student DeletedStudent, List<GroupDetail> UpdatedGroupDetails);

public class DeleteStudentRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IOrdinalService ordinalService
) : IRequestHandler<DeleteStudentRequest, DeleteStudentResponse>
{
  public async Task<DeleteStudentResponse> Handle(
    DeleteStudentRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var studentGroups =
        await dbContext
          .StudentGroups.Where(sg => sg.StudentId == request.StudentId)
          .ToListAsync(cancellationToken) ?? throw new Exception();

      var groupIds = studentGroups.Select(sg => sg.GroupId) ?? throw new Exception();

      var studentDTO =
        await dbContext
          .Students.Where(s => s.Id == request.StudentId && s.ClassroomId == request.ClassroomId)
          .SingleOrDefaultAsync(cancellationToken) ?? throw new Exception();

      dbContext.Students.Remove(studentDTO);
      await dbContext.SaveChangesAsync(cancellationToken);

      var updatedGroups = await ordinalService.RecalculateStudentOrdinals(
        account.Id,
        studentDTO.ClassroomId,
        groupIds.ToList(),
        cancellationToken
      );

      transaction.Commit();

      return new DeleteStudentResponse(studentDTO.ToStudent(), updatedGroups);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
