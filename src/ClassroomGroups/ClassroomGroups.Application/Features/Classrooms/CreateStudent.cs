using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateStudentRequest(Guid ClassroomId, Guid ConfigurationId, Guid GroupId)
  : IRequest<CreateStudentResponse> { }

public record CreateStudentResponse(ConfigurationDetail UpdatedConfigurationDetail) { }

public class CreateStudentRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IGetDetailService getConfigurationDetailService
) : IRequestHandler<CreateStudentRequest, CreateStudentResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  readonly IGetDetailService _getConfigurationDetailService = getConfigurationDetailService;

  public async Task<CreateStudentResponse> Handle(
    CreateStudentRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    var classroomDTO =
      await _dbContext
        .Classrooms.Where(c => c.Id == request.ClassroomId)
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    var studentDTO = new StudentDTO()
    {
      Id = Guid.NewGuid(),
      ClassroomKey = classroomDTO.Key,
      ClassroomId = classroomDTO.Id,
    };
    var studentEntity = await _dbContext.Students.AddAsync(studentDTO, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    var groupDTO =
      await _dbContext
        .Groups.Where(g => g.Id == request.GroupId)
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    var existingStudentGroups = await _dbContext
      .StudentGroups.Where(sg => sg.GroupId == request.GroupId)
      .ToListAsync(cancellationToken);

    var studentGroupDTO = new StudentGroupDTO()
    {
      Id = Guid.NewGuid(),
      GroupId = request.GroupId,
      StudentId = studentEntity.Entity.Id,
      GroupKey = groupDTO.Key,
      StudentKey = studentEntity.Entity.Key,
      Ordinal = existingStudentGroups.Count
    };

    var studentGroupEntity = await _dbContext.StudentGroups.AddAsync(
      studentGroupDTO,
      cancellationToken
    );

    await _dbContext.SaveChangesAsync(cancellationToken);

    var studentFieldDTOs = await _dbContext
      .Fields.Where(f => f.ClassroomId == request.ClassroomId)
      .Select(f => new StudentFieldDTO
      {
        FieldId = f.Id,
        FieldKey = f.Key,
        StudentId = studentEntity.Entity.Id,
        StudentKey = studentEntity.Entity.Key,
      })
      .ToListAsync(cancellationToken);

    await _dbContext.StudentFields.AddRangeAsync(studentFieldDTOs, cancellationToken);

    var configurationDetail =
      await _getConfigurationDetailService.GetConfigurationDetail(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      ) ?? throw new Exception();

    return new CreateStudentResponse(configurationDetail);
  }
}
