using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record UpsertStudentFieldRequest(
  Guid ClassroomId,
  Guid StudentId,
  Guid FieldId,
  string Value
) : IRequest<UpsertStudentFieldResponse>, IRequiredUserAccount
{
  public EntityIds GetEntityIds() =>
    new()
    {
      ClassroomIds = [ClassroomId],
      StudentIds = [StudentId],
      FieldIds = [FieldId]
    };
}

public record UpsertStudentFieldResponse(string UpsertedValue) { }

public class UpsertStudentFieldRequestHandler(
  AccountRequiredCache authBehaviorCache,
  ClassroomGroupsContext dbContext
) : IRequestHandler<UpsertStudentFieldRequest, UpsertStudentFieldResponse>
{
  public async Task<UpsertStudentFieldResponse> Handle(
    UpsertStudentFieldRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var studentFieldDTO = await dbContext
        .StudentFields.Where(sf =>
          sf.StudentDTO.ClassroomId == request.ClassroomId
          && sf.StudentId == request.StudentId
          && sf.FieldId == request.FieldId
        )
        .FirstOrDefaultAsync(cancellationToken);

      var value = request.Value.Trim();

      if (studentFieldDTO != null)
      {
        studentFieldDTO.Value = value;
        dbContext.StudentFields.Update(studentFieldDTO);
      }
      else
      {
        var studentDTO =
          await dbContext
            .Students.Where(s => s.Id == request.StudentId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

        var fieldDTO =
          await dbContext
            .Fields.Where(f => f.Id == request.FieldId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

        studentFieldDTO = new StudentFieldDTO
        {
          StudentId = request.StudentId,
          StudentKey = studentDTO.Key,
          FieldId = request.FieldId,
          FieldKey = fieldDTO.Key,
          Value = value,
          Id = Guid.NewGuid()
        };

        dbContext.StudentFields.Add(studentFieldDTO);
      }

      await dbContext.SaveChangesAsync(cancellationToken);

      transaction.Commit();

      return new UpsertStudentFieldResponse(studentFieldDTO.Value);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
