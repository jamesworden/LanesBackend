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
  public async Task<CreateStudentResponse> Handle(
    CreateStudentRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    var existingStudentDTOs = await dbContext
      .Students.Where(s => s.ClassroomId == request.ClassroomId)
      .ToListAsync(cancellationToken);

    if (existingStudentDTOs.Count >= account.Subscription.MaxStudentsPerClassroom)
    {
      throw new Exception();
    }

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var classroomDTO =
        await dbContext
          .Classrooms.Where(c => c.Id == request.ClassroomId)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      var configurationDTO =
        await dbContext
          .Configurations.Where(c => c.Id == request.ConfigurationId)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      var studentId = Guid.NewGuid();

      var studentDTO = new StudentDTO()
      {
        Id = studentId,
        ClassroomKey = classroomDTO.Key,
        ClassroomId = classroomDTO.Id,
      };
      var studentEntity = await dbContext.Students.AddAsync(studentDTO, cancellationToken);

      await dbContext.SaveChangesAsync(cancellationToken);

      var groupId =
        request.GroupId ?? configurationDTO.DefaultGroupId ?? throw new InvalidOperationException();

      var groupDTO =
        await dbContext.Groups.Where(g => g.Id == groupId).FirstOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException();

      var existingStudentGroups = await dbContext
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

      var studentGroupEntity = await dbContext.StudentGroups.AddAsync(
        studentGroupDTO,
        cancellationToken
      );

      await dbContext.SaveChangesAsync(cancellationToken);

      var otherConfigurationDTOs = await dbContext
        .Configurations.Where(c =>
          c.ClassroomId == request.ClassroomId && c.Id != request.ConfigurationId
        )
        .ToListAsync(cancellationToken);

      var studentGroupDTOs = await Task.WhenAll(
        otherConfigurationDTOs.Select(async c =>
        {
          var groupId = c.DefaultGroupId ?? throw new InvalidOperationException();
          var groupKey = c.DefaultGroupKey ?? throw new InvalidOperationException();
          var existingStudentGroups =
            await dbContext
              .StudentGroups.Where(sg => sg.GroupId == c.DefaultGroupId)
              .ToListAsync(cancellationToken) ?? throw new InvalidOperationException();

          return new StudentGroupDTO()
          {
            GroupId = c.DefaultGroupId ?? throw new InvalidOperationException(),
            GroupKey = c.DefaultGroupKey ?? throw new InvalidOperationException(),
            StudentId = studentDTO.Id,
            StudentKey = studentDTO.Key,
            Ordinal = existingStudentGroups.Count,
            Id = Guid.NewGuid(),
          };
        })
      );

      await dbContext.AddRangeAsync(studentGroupDTOs, cancellationToken);

      await dbContext.SaveChangesAsync(cancellationToken);

      var studentDetails =
        await detailService.GetStudentDetails(
          account.Id,
          request.ClassroomId,
          request.ConfigurationId,
          cancellationToken
        ) ?? throw new InvalidOperationException();

      var studentDetail =
        studentDetails.Find(s => s.Id == studentId) ?? throw new InvalidOperationException();

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
