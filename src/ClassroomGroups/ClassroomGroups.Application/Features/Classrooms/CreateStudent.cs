using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateStudentRequest(Guid ClassroomId, Guid ConfigurationId, Guid? GroupId)
  : IRequest<CreateStudentResponse>,
    IRequiredUserAccount { }

public record CreateStudentResponse(StudentDetail CreatedStudentDetail) { }

public class CreateStudentRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<CreateStudentRequest, CreateStudentResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AccountRequiredCache _authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  public async Task<CreateStudentResponse> Handle(
    CreateStudentRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;

    var existingStudentDTOs = await _dbContext
      .Students.Where(s => s.ClassroomId == request.ClassroomId)
      .ToListAsync(cancellationToken);

    if (existingStudentDTOs.Count >= account.Subscription.MaxStudentsPerClassroom)
    {
      throw new Exception();
    }

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var classroomDTO =
        await _dbContext
          .Classrooms.Where(c => c.Id == request.ClassroomId)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var configurationDTO =
        await _dbContext
          .Configurations.Where(c => c.Id == request.ConfigurationId)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var studentId = Guid.NewGuid();

      var studentDTO = new StudentDTO()
      {
        Id = studentId,
        ClassroomKey = classroomDTO.Key,
        ClassroomId = classroomDTO.Id,
      };
      var studentEntity = await _dbContext.Students.AddAsync(studentDTO, cancellationToken);

      await _dbContext.SaveChangesAsync(cancellationToken);

      var groupId = request.GroupId ?? configurationDTO.DefaultGroupId ?? throw new Exception();

      var groupDTO =
        await _dbContext.Groups.Where(g => g.Id == groupId).FirstOrDefaultAsync(cancellationToken)
        ?? throw new Exception();

      var existingStudentGroups = await _dbContext
        .StudentGroups.Where(sg => sg.GroupId == groupId)
        .ToListAsync(cancellationToken);

      var studentGroupDTO = new StudentGroupDTO()
      {
        Id = Guid.NewGuid(),
        GroupId = groupId,
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

      var otherConfigurationDTOs = await _dbContext
        .Configurations.Where(c =>
          c.ClassroomId == request.ClassroomId && c.Id != request.ConfigurationId
        )
        .ToListAsync(cancellationToken);

      var studentGroupDTOs = await Task.WhenAll(
        otherConfigurationDTOs.Select(async c =>
        {
          var groupId = c.DefaultGroupId ?? throw new Exception();
          var groupKey = c.DefaultGroupKey ?? throw new Exception();
          var existingStudentGroups =
            await _dbContext
              .StudentGroups.Where(sg => sg.GroupId == c.DefaultGroupId)
              .ToListAsync(cancellationToken) ?? throw new Exception();

          return new StudentGroupDTO()
          {
            GroupId = c.DefaultGroupId ?? throw new Exception(),
            GroupKey = c.DefaultGroupKey ?? throw new Exception(),
            StudentId = studentDTO.Id,
            StudentKey = studentDTO.Key,
            Ordinal = existingStudentGroups.Count,
            Id = Guid.NewGuid(),
          };
        })
      );

      await _dbContext.AddRangeAsync(studentGroupDTOs, cancellationToken);

      await _dbContext.SaveChangesAsync(cancellationToken);

      var studentDetails =
        await _detailService.GetStudentDetails(
          account.Id,
          request.ClassroomId,
          request.ConfigurationId,
          cancellationToken
        ) ?? throw new Exception();

      var studentDetail = studentDetails.Find(s => s.Id == studentId) ?? throw new Exception();

      transaction.Commit();

      return new CreateStudentResponse(studentDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
