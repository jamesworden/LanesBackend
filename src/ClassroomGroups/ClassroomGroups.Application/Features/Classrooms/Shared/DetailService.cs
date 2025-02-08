using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Shared;

public interface IDetailService
{
  public Task<ConfigurationDetail> GetConfigurationDetail(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  );

  public Task<List<GroupDetail>> GetGroupDetails(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  );

  public Task<List<GroupDetail>> GetGroupDetails(
    Guid accountId,
    Guid classroomId,
    List<Guid> groupIds,
    CancellationToken cancellationToken
  );

  public Task<List<StudentDetail>> GetStudentDetails(
    Guid accountId,
    Guid classroomId,
    Guid? configurationId,
    CancellationToken cancellationToken
  );

  public Task<List<ColumnDetail>> GetColumnDetails(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  );
}

public class DetailService(ClassroomGroupsContext dbContext) : IDetailService
{
  public async Task<ConfigurationDetail> GetConfigurationDetail(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  )
  {
    var groupDetails = await GetGroupDetails(
      accountId,
      classroomId,
      configurationId,
      cancellationToken
    );

    var columnDetails = await GetColumnDetails(
      accountId,
      classroomId,
      configurationId,
      cancellationToken
    );

    var classroomIds = (
      await dbContext.Classrooms.Where(c => c.AccountId == accountId).ToListAsync(cancellationToken)
    )
      .Select(c => c.Id)
      .ToList();

    var configurationDetail =
      (
        await dbContext
          .Configurations.Where(c =>
            c.Id == configurationId
            && c.ClassroomId == classroomId
            && classroomIds.Contains(classroomId)
          )
          .Select(c => new ConfigurationDetailDTO(
            c.Id,
            c.ClassroomId,
            c.DefaultGroupId ?? Guid.Empty,
            c.Label,
            c.Description
          ))
          .FirstOrDefaultAsync(cancellationToken)
      )?.ToConfigurationDetail(groupDetails, columnDetails) ?? throw new Exception();

    if (configurationDetail.DefaultGroupId.Equals(Guid.Empty))
    {
      throw new Exception("Configuration has no default group.");
    }

    return configurationDetail;
  }

  public async Task<List<GroupDetail>> GetGroupDetails(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  )
  {
    var studentDetails = await GetStudentDetails(
      accountId,
      classroomId,
      configurationId,
      cancellationToken
    );

    var groupDetails =
      (
        await dbContext
          .Groups.Where(g => g.ConfigurationId == configurationId)
          .Select(g => new GroupDetailDTO(g.Id, g.ConfigurationId, g.Label, g.Ordinal, g.IsLocked))
          .ToListAsync(cancellationToken)
      )
        .Select(g => g.ToGroupDetail(studentDetails.Where(s => s.GroupId == g.Id).ToList()))
        .OrderBy(g => g.Ordinal)
        .ToList() ?? [];

    return groupDetails;
  }

  public async Task<List<GroupDetail>> GetGroupDetails(
    Guid accountId,
    Guid classroomId,
    List<Guid> groupIds,
    CancellationToken cancellationToken
  )
  {
    var studentDetails = await GetStudentDetails(accountId, classroomId, null, cancellationToken);

    return (
        await dbContext
          .Groups.Where(g => groupIds.Contains(g.Id))
          .Select(g => new GroupDetailDTO(g.Id, g.ConfigurationId, g.Label, g.Ordinal, g.IsLocked))
          .ToListAsync(cancellationToken)
      )
        .Select(g => g.ToGroupDetail(studentDetails.Where(s => s.GroupId == g.Id).ToList()))
        .OrderBy(g => g.Ordinal)
        .ToList() ?? [];
  }

  public async Task<List<ColumnDetail>> GetColumnDetails(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  )
  {
    return (
      await dbContext
        .Columns.Where(c => c.ConfigurationId == configurationId)
        .Join(
          dbContext.Fields,
          column => column.FieldId,
          field => field.Id,
          (column, field) => new { column, field }
        )
        .Select(c => new ColumnDetailDTO(
          c.column.Id,
          c.column.ConfigurationId,
          c.column.FieldId,
          c.column.Ordinal,
          c.column.Sort,
          c.column.Enabled,
          c.field.Type,
          c.field.Label
        ))
        .ToListAsync(cancellationToken)
    )
      .Select(c => c.ToColumnDetail())
      .OrderBy(c => c.Ordinal)
      .ToList();
  }

  public async Task<List<StudentDetail>> GetStudentDetails(
    Guid accountId,
    Guid classroomId,
    Guid? configurationId,
    CancellationToken cancellationToken
  )
  {
    var studentDetails = await dbContext
      .StudentGroups.Where(sg =>
        configurationId == null || sg.GroupDTO.ConfigurationId == configurationId
      )
      .Join(
        dbContext.Students,
        sg => sg.StudentId,
        s => s.Id,
        (sg, s) => new { Student = s, StudentGroup = sg }
      )
      .ToListAsync(cancellationToken);

    var studentDetailsWithFields = studentDetails
      .Select(x => new StudentDetailDTO(
        x.Student.Id,
        x.StudentGroup.GroupId,
        x.StudentGroup.Ordinal,
        x.StudentGroup.Id,
        dbContext
          .StudentFields.Where(sf => sf.StudentId == x.Student.Id)
          .ToDictionary(sf => sf.FieldId, sf => sf.Value)
      ))
      .Select(s => s.ToStudentDetail())
      .OrderBy(s => s.Ordinal)
      .ToList();

    return studentDetailsWithFields;
  }
}
