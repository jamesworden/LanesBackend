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

  public Task<List<StudentDetail>> GetStudentDetails(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  );
}

public class DetailService(ClassroomGroupsContext dbContext) : IDetailService
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

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

    var columnDetails = (
      await _dbContext
        .Columns.Where(c => c.ConfigurationId == configurationId)
        .Join(
          _dbContext.Fields,
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
      .ToList();

    var classroomIds = (
      await _dbContext.Classrooms.Where(c => c.AccountId == accountId).ToListAsync()
    )
      .Select(c => c.Id)
      .ToList();

    var unassignedStudents = (
      await _dbContext
        .Students.Where(s =>
          !_dbContext.StudentGroups.Any(sg =>
            sg.StudentId == s.Id && sg.GroupDTO.ConfigurationId == configurationId
          )
        )
        .Select(s => new
        {
          StudentDTO = s,
          StudentFields = _dbContext
            .StudentFields.Where(sf => sf.StudentId == s.Id)
            .Select(sf => new { sf.FieldId, sf.Value })
            .ToList()
        })
        .ToListAsync(cancellationToken)
    )
      .Select(s => new
      {
        Student = s.StudentDTO.ToStudent(),
        Fields = s.StudentFields.ToDictionary(sf => sf.FieldId, sf => sf.Value)
      })
      .Select(s => s.Student.WithFields(s.Fields))
      .ToList();

    var configurationDetail =
      (
        await _dbContext
          .Configurations.Where(c =>
            c.Id == configurationId
            && c.ClassroomId == classroomId
            && classroomIds.Contains(classroomId)
          )
          .Select(c => new ConfigurationDetailDTO(c.Id, c.ClassroomId, c.Label, c.Description))
          .FirstOrDefaultAsync(cancellationToken)
      )?.ToConfigurationDetail(groupDetails, columnDetails, unassignedStudents)
      ?? throw new Exception();

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

    return (
        await _dbContext
          .Groups.Where(g => g.ConfigurationId == configurationId)
          .Select(g => new GroupDetailDTO(g.Id, g.ConfigurationId, g.Label, g.Ordinal))
          .ToListAsync(cancellationToken)
      )
        .Select(g => g.ToGroupDetail(studentDetails))
        .ToList() ?? [];
  }

  public async Task<List<StudentDetail>> GetStudentDetails(
    Guid accountId,
    Guid classroomId,
    Guid configurationId,
    CancellationToken cancellationToken
  )
  {
    return (
        await _dbContext
          .StudentGroups.Where(sg => sg.GroupDTO.ConfigurationId == configurationId)
          .Join(
            _dbContext.Students,
            sg => sg.StudentId,
            s => s.Id,
            (sg, s) => new { Student = s, StudentGroup = sg }
          )
          .Select(x => new StudentDetailDTO(
            x.Student.Id,
            x.StudentGroup.GroupId,
            x.StudentGroup.Ordinal
          ))
          .ToListAsync(cancellationToken)
      )
        .Select(sg => sg.ToStudentDetail())
        .ToList() ?? [];
  }
}
