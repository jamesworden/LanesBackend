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
) : IRequest<UpsertStudentFieldResponse> { }

public record UpsertStudentFieldResponse(string UpsertedValue) { }

public class UpsertStudentFieldRequestHandler(
  AuthBehaviorCache authBehaviorCache,
  ClassroomGroupsContext classroomGroupsContext
) : IRequestHandler<UpsertStudentFieldRequest, UpsertStudentFieldResponse>
{
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly ClassroomGroupsContext _dbContext = classroomGroupsContext;

  public async Task<UpsertStudentFieldResponse> Handle(
    UpsertStudentFieldRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var studentFieldDTO = await _dbContext
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
        _dbContext.StudentFields.Update(studentFieldDTO);
      }
      else
      {
        var studentDTO =
          await _dbContext
            .Students.Where(s => s.Id == request.StudentId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

        var fieldDTO =
          await _dbContext
            .Fields.Where(f => f.Id == request.FieldId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

        studentFieldDTO = new StudentFieldDTO
        {
          StudentId = request.StudentId,
          StudentKey = studentDTO.Key,
          FieldId = request.FieldId,
          FieldKey = fieldDTO.Key,
          Value = value,
          Id = Guid.NewGuid()
        };

        _dbContext.StudentFields.Add(studentFieldDTO);
      }

      await _dbContext.SaveChangesAsync(cancellationToken);

      return new UpsertStudentFieldResponse(studentFieldDTO.Value);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
