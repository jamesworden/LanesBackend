using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateClassroomRequest(string? Label, string? Description)
  : IRequest<CreateClassroomResponse> { }

public record CreateClassroomResponse(ClassroomDetail CreatedClassroomDetail) { }

public class CreateClassroomRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IConfigurationService configurationService
) : IRequestHandler<CreateClassroomRequest, CreateClassroomResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly IConfigurationService _configurationService = configurationService;

  readonly string DEFAULT_CLASSROOM_LABEL = "Untitled";

  readonly string DEFAULT_FIRST_CONFIGURATION_LABEL = "Configuration 1";

  public async Task<CreateClassroomResponse> Handle(
    CreateClassroomRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var classroomDTO = new ClassroomDTO()
      {
        Id = Guid.NewGuid(),
        Label = request.Label ?? DEFAULT_CLASSROOM_LABEL,
        Description = request.Description ?? "",
        AccountKey = account.Key,
        AccountId = account.Id
      };
      var classroomEntity = await _dbContext.Classrooms.AddAsync(classroomDTO, cancellationToken);
      await _dbContext.SaveChangesAsync(cancellationToken);
      var classroom = (classroomEntity.Entity?.ToClassroom()) ?? throw new Exception();

      var configuration = await _configurationService.CreateConfiguration(
        account.Id,
        classroom.Id,
        DEFAULT_FIRST_CONFIGURATION_LABEL,
        cancellationToken
      );

      var studentDTOs = new List<StudentDTO>();

      var studentDTO1 = new StudentDTO()
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
      };
      var studentDTO2 = new StudentDTO()
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
      };
      var studentDTO3 = new StudentDTO()
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
      };

      await _dbContext.Students.AddRangeAsync(
        [studentDTO1, studentDTO2, studentDTO3],
        cancellationToken
      );

      var defaultGroupStudentDTO1 = new StudentDTO()
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
      };
      var defaultGroupStudentDTO2 = new StudentDTO()
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
      };
      var defaultGroupStudentDTO3 = new StudentDTO()
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
      };

      await _dbContext.Students.AddRangeAsync(
        [defaultGroupStudentDTO1, defaultGroupStudentDTO2, defaultGroupStudentDTO3],
        cancellationToken
      );

      // Assign students to default group

      var fieldDTO1 = new FieldDTO
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
        Label = "First Name",
        Type = FieldType.TEXT
      };
      var fieldDTO2 = new FieldDTO
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
        Label = "Last Name",
        Type = FieldType.TEXT
      };
      var fieldDTO3 = new FieldDTO
      {
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Id = Guid.NewGuid(),
        Label = "Homework #1",
        Type = FieldType.NUMBER
      };

      await _dbContext.Fields.AddRangeAsync([fieldDTO1, fieldDTO2, fieldDTO3], cancellationToken);

      await _dbContext.SaveChangesAsync(cancellationToken);

      var configurationDTO =
        await _dbContext
          .Configurations.Where(c => c.Id == configuration.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var fieldDTO1withKey =
        await _dbContext
          .Fields.Where(f => f.Id == fieldDTO1.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var fieldDTO2withKey =
        await _dbContext
          .Fields.Where(f => f.Id == fieldDTO2.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var fieldDTO3withKey =
        await _dbContext
          .Fields.Where(f => f.Id == fieldDTO3.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var columnDTO1 = new ColumnDTO
      {
        ConfigurationId = configurationDTO.Id,
        ConfigurationKey = configurationDTO.Key,
        Id = Guid.NewGuid(),
        Enabled = true,
        FieldKey = fieldDTO1.Key,
        FieldId = fieldDTO1.Id,
        Ordinal = 0,
        Sort = ColumnSort.NONE
      };
      var columnDTO2 = new ColumnDTO
      {
        ConfigurationId = configurationDTO.Id,
        ConfigurationKey = configurationDTO.Key,
        Id = Guid.NewGuid(),
        Enabled = true,
        FieldKey = fieldDTO2.Key,
        FieldId = fieldDTO2.Id,
        Ordinal = 1,
        Sort = ColumnSort.NONE
      };
      var columnDTO3 = new ColumnDTO
      {
        ConfigurationId = configurationDTO.Id,
        ConfigurationKey = configurationDTO.Key,
        Id = Guid.NewGuid(),
        Enabled = true,
        FieldKey = fieldDTO3.Key,
        FieldId = fieldDTO3.Id,
        Ordinal = 2,
        Sort = ColumnSort.NONE
      };

      await _dbContext.Columns.AddRangeAsync(
        [columnDTO1, columnDTO2, columnDTO3],
        cancellationToken
      );

      var groupId = Guid.NewGuid();

      await _dbContext.Groups.AddAsync(
        new()
        {
          Id = groupId,
          Label = "Group 1",
          Ordinal = 1,
          ConfigurationId = configurationDTO.Id,
          ConfigurationKey = configurationDTO.Key
        },
        cancellationToken
      );

      await _dbContext.SaveChangesAsync(cancellationToken);

      var groupDTO =
        await _dbContext.Groups.Where(g => g.Id == groupId).FirstOrDefaultAsync(cancellationToken)
        ?? throw new Exception();

      var studentDTO1withKey =
        await _dbContext
          .Students.Where(s => studentDTO1.Id == s.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var studentDTO2withKey =
        await _dbContext
          .Students.Where(s => studentDTO2.Id == s.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var studentDTO3withKey =
        await _dbContext
          .Students.Where(s => studentDTO3.Id == s.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var defaultGroupStudentDTO1withKey =
        await _dbContext
          .Students.Where(s => defaultGroupStudentDTO1.Id == s.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var defaultGroupStudentDTO2withKey =
        await _dbContext
          .Students.Where(s => defaultGroupStudentDTO2.Id == s.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var defaultGroupStudentDTO3withKey =
        await _dbContext
          .Students.Where(s => defaultGroupStudentDTO3.Id == s.Id)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var defaultGroupId = configurationDTO.DefaultGroupId ?? throw new Exception();
      var defaultGroupKey = configurationDTO.DefaultGroupKey ?? throw new Exception();

      await _dbContext.StudentGroups.AddRangeAsync(
        [
          new()
          {
            GroupId = groupDTO.Id,
            GroupKey = groupDTO.Key,
            Ordinal = 0,
            StudentId = studentDTO1withKey.Id,
            StudentKey = studentDTO1withKey.Key,
            Id = Guid.NewGuid()
          },
          new()
          {
            GroupId = groupDTO.Id,
            GroupKey = groupDTO.Key,
            Ordinal = 0,
            StudentId = studentDTO2withKey.Id,
            StudentKey = studentDTO2withKey.Key,
            Id = Guid.NewGuid()
          },
          new()
          {
            GroupId = groupDTO.Id,
            GroupKey = groupDTO.Key,
            Ordinal = 0,
            StudentId = studentDTO3withKey.Id,
            StudentKey = studentDTO3withKey.Key,
            Id = Guid.NewGuid()
          },
          new()
          {
            GroupId = defaultGroupId,
            GroupKey = defaultGroupKey,
            Ordinal = 0,
            StudentId = defaultGroupStudentDTO1withKey.Id,
            StudentKey = defaultGroupStudentDTO1withKey.Key,
            Id = Guid.NewGuid()
          },
          new()
          {
            GroupId = defaultGroupId,
            GroupKey = defaultGroupKey,
            Ordinal = 0,
            StudentId = defaultGroupStudentDTO2withKey.Id,
            StudentKey = defaultGroupStudentDTO2withKey.Key,
            Id = Guid.NewGuid()
          },
          new()
          {
            GroupId = defaultGroupId,
            GroupKey = defaultGroupKey,
            Ordinal = 0,
            StudentId = defaultGroupStudentDTO3withKey.Id,
            StudentKey = defaultGroupStudentDTO3withKey.Key,
            Id = Guid.NewGuid()
          }
        ],
        cancellationToken
      );

      await _dbContext.SaveChangesAsync(cancellationToken);

      var fieldDetails =
        (
          await _dbContext
            .Fields.Where(f => f.ClassroomId == classroom.Id)
            .Select(f => new FieldDetailDTO(f.Id, f.ClassroomId, f.Label, f.Type))
            .ToListAsync(cancellationToken)
        )
          .Select(f => f.ToFieldDetail())
          .ToList() ?? [];

      var createdClassroomDetail = classroom.ToClassroomDetail(fieldDetails);

      transaction.Commit();

      return new CreateClassroomResponse(createdClassroomDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
