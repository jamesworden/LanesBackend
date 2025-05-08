using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record ImportClassroomRequest(
  string Label,
  List<ImportFieldRequest> Fields,
  List<Dictionary<string, string>> Students
) : IRequest<ImportClassroomResponse>, IRequiredUserAccount
{
  public EntityIds GetEntityIds() => new();
}

public record ImportFieldRequest(string Label, FieldType Type);

public record ImportClassroomResponse(ClassroomDetail ImportedClassroomDetail) { }

public class ImportClassroomRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IConfigurationService configurationService
) : IRequestHandler<ImportClassroomRequest, ImportClassroomResponse>
{
  readonly string DEFAULT_CONFIGURATION_LABEL = "Configuration 1";

  public async Task<ImportClassroomResponse> Handle(
    ImportClassroomRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    var existingClassroomDTOs = await dbContext
      .Classrooms.Where(c => c.AccountId == account.Id)
      .ToListAsync(cancellationToken);

    if (existingClassroomDTOs.Count >= account.Subscription.MaxClassrooms)
    {
      throw new Exception("Maximum number of classrooms reached for this account.");
    }

    // Validate field count against subscription limits
    if (request.Fields.Count > account.Subscription.MaxFieldsPerClassroom)
    {
      throw new Exception(
        $"Maximum number of fields ({account.Subscription.MaxFieldsPerClassroom}) exceeded."
      );
    }

    // Validate student count against subscription limits
    if (request.Students.Count > account.Subscription.MaxStudentsPerClassroom)
    {
      throw new Exception(
        $"Maximum number of students ({account.Subscription.MaxStudentsPerClassroom}) exceeded."
      );
    }

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      // Create new classroom
      var classroomDTO = new ClassroomDTO()
      {
        Id = Guid.NewGuid(),
        Label = request.Label,
        Description = "",
        AccountKey = account.Key,
        AccountId = account.Id
      };

      var classroomEntity = await dbContext.Classrooms.AddAsync(classroomDTO, cancellationToken);
      await dbContext.SaveChangesAsync(cancellationToken);

      var classroom =
        (classroomEntity.Entity?.ToClassroom())
        ?? throw new InvalidOperationException("Failed to create classroom");

      // Create configuration
      var configuration = await configurationService.CreateConfiguration(
        account.Id,
        classroom.Id,
        DEFAULT_CONFIGURATION_LABEL,
        cancellationToken
      );

      // Create fields from the request
      var fieldDTOs = new List<FieldDTO>();
      foreach (var field in request.Fields)
      {
        var fieldDTO = new FieldDTO
        {
          ClassroomId = classroomDTO.Id,
          ClassroomKey = classroomDTO.Key,
          Id = Guid.NewGuid(),
          Label = field.Label,
          Type = field.Type
        };
        fieldDTOs.Add(fieldDTO);
      }

      await dbContext.Fields.AddRangeAsync(fieldDTOs, cancellationToken);
      await dbContext.SaveChangesAsync(cancellationToken);

      // Create students from the request
      var studentDTOs = new List<StudentDTO>();
      foreach (var _ in request.Students)
      {
        var studentDTO = new StudentDTO()
        {
          ClassroomId = classroomDTO.Id,
          ClassroomKey = classroomDTO.Key,
          Id = Guid.NewGuid(),
        };
        studentDTOs.Add(studentDTO);
      }

      await dbContext.Students.AddRangeAsync(studentDTOs, cancellationToken);
      await dbContext.SaveChangesAsync(cancellationToken);

      // Refresh student DTOs to get the generated keys
      var refreshedStudentDTOs = new List<StudentDTO>();
      foreach (var studentDTO in studentDTOs)
      {
        var refreshedStudent =
          await dbContext
            .Students.Where(s => s.Id == studentDTO.Id)
            .FirstOrDefaultAsync(cancellationToken)
          ?? throw new InvalidOperationException("Student not found");
        refreshedStudentDTOs.Add(refreshedStudent);
      }

      // Refresh field DTOs to get the generated keys
      var refreshedFieldDTOs = new List<FieldDTO>();
      foreach (var fieldDTO in fieldDTOs)
      {
        var refreshedField =
          await dbContext
            .Fields.Where(f => f.Id == fieldDTO.Id)
            .FirstOrDefaultAsync(cancellationToken)
          ?? throw new InvalidOperationException("Field not found");
        refreshedFieldDTOs.Add(refreshedField);
      }

      // Get configuration with updated keys
      var configurationDTO =
        await dbContext
          .Configurations.Where(c => c.Id == configuration.Id)
          .FirstOrDefaultAsync(cancellationToken)
        ?? throw new InvalidOperationException("Configuration not found");

      // Create columns for each field
      var columnDTOs = new List<ColumnDTO>();
      for (int i = 0; i < refreshedFieldDTOs.Count; i++)
      {
        var field = refreshedFieldDTOs[i];
        var columnDTO = new ColumnDTO
        {
          ConfigurationId = configurationDTO.Id,
          ConfigurationKey = configurationDTO.Key,
          Id = Guid.NewGuid(),
          Enabled = true,
          FieldKey = field.Key,
          FieldId = field.Id,
          Ordinal = i,
          Sort = ColumnSort.NONE
        };
        columnDTOs.Add(columnDTO);
      }

      await dbContext.Columns.AddRangeAsync(columnDTOs, cancellationToken);
      await dbContext.SaveChangesAsync(cancellationToken);

      // Get default group
      var defaultGroupId =
        configurationDTO.DefaultGroupId
        ?? throw new InvalidOperationException("Default group ID not found");
      var defaultGroupKey =
        configurationDTO.DefaultGroupKey
        ?? throw new InvalidOperationException("Default group key not found");

      // Create student group associations
      var studentGroupDTOs = new List<StudentGroupDTO>();
      for (int i = 0; i < refreshedStudentDTOs.Count; i++)
      {
        var student = refreshedStudentDTOs[i];
        var studentGroupDTO = new StudentGroupDTO
        {
          GroupId = defaultGroupId,
          GroupKey = defaultGroupKey,
          Ordinal = i,
          StudentId = student.Id,
          StudentKey = student.Key,
          Id = Guid.NewGuid()
        };
        studentGroupDTOs.Add(studentGroupDTO);
      }

      await dbContext.StudentGroups.AddRangeAsync(studentGroupDTOs, cancellationToken);
      await dbContext.SaveChangesAsync(cancellationToken);

      // Create student field associations and data
      var studentFieldDTOs = new List<StudentFieldDTO>();

      for (int studentIndex = 0; studentIndex < request.Students.Count; studentIndex++)
      {
        var studentData = request.Students[studentIndex];
        var student = refreshedStudentDTOs[studentIndex];

        foreach (var field in refreshedFieldDTOs)
        {
          // Create StudentFieldDTO (the association between student and field)
          var studentFieldDTO = new StudentFieldDTO
          {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            StudentKey = student.Key,
            FieldId = field.Id,
            FieldKey = field.Key,
            Value = studentData.TryGetValue(field.Label, out var value) ? value : ""
          };
          studentFieldDTOs.Add(studentFieldDTO);
        }
      }

      await dbContext.StudentFields.AddRangeAsync(studentFieldDTOs, cancellationToken);
      await dbContext.SaveChangesAsync(cancellationToken);

      // Get field details for response
      var fieldDetails =
        (
          await dbContext
            .Fields.Where(f => f.ClassroomId == classroom.Id)
            .Select(f => new FieldDetailDTO(f.Id, f.ClassroomId, f.Label, f.Type))
            .ToListAsync(cancellationToken)
        )
          .Select(f => f.ToFieldDetail())
          .ToList() ?? [];

      var importedClassroomDetail = classroom.ToClassroomDetail(fieldDetails);

      transaction.Commit();

      return new ImportClassroomResponse(importedClassroomDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
