using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteStudentRequest(Guid ClassroomId, Guid StudentId)
  : IRequest<DeleteStudentResponse>;

public record DeleteStudentResponse(Student DeletedStudent, List<GroupDetail> UpdatedGroups);

public class DeleteStudentRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService,
  IOrdinalService ordinalService
) : IRequestHandler<DeleteStudentRequest, DeleteStudentResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  readonly IOrdinalService _ordinalService = ordinalService;

  public async Task<DeleteStudentResponse> Handle(
    DeleteStudentRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var studentGroups =
        await _dbContext
          .StudentGroups.Where(sg => sg.StudentId == request.StudentId)
          .ToListAsync(cancellationToken) ?? throw new Exception();

      var groupIds = studentGroups.Select(sg => sg.GroupId) ?? throw new Exception();

      var studentDTO =
        await _dbContext
          .Students.Where(s => s.Id == request.StudentId && s.ClassroomId == request.ClassroomId)
          .SingleOrDefaultAsync(cancellationToken) ?? throw new Exception();

      _dbContext.Students.Remove(studentDTO);
      await _dbContext.SaveChangesAsync(cancellationToken);

      var updatedGroups = await _ordinalService.RecalculateStudentOrdinals(
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
