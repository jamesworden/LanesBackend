using System.Security.Claims;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class GetClassroomDetailsRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetClassroomDetailsRequest, ClassroomDetails?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<ClassroomDetails?> Handle(
    GetClassroomDetailsRequest request,
    CancellationToken cancellationToken
  )
  {
    if (_httpContextAccessor.HttpContext is null)
    {
      return null;
    }
    var googleNameIdentifier = _httpContextAccessor
      .HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)
      ?.Value;
    if (googleNameIdentifier is null)
    {
      return null;
    }
    var accountDTO = await _dbContext.Accounts.FirstOrDefaultAsync(
      a => a.GoogleNameIdentifier == googleNameIdentifier,
      cancellationToken
    );
    if (accountDTO is null)
    {
      return null;
    }

    // This can be optimized in the future.

    List<ClassroomDTO> classroomDTOs =
      await _dbContext
        .Classrooms.Where(c => c.AccountKey == accountDTO.Key)
        .ToListAsync(cancellationToken) ?? [];

    List<StudentDTO> studentDTOs =
      await _dbContext
        .Students.Where(s => classroomDTOs.Select(c => c.Key).Contains(s.ClassroomKey))
        .ToListAsync(cancellationToken) ?? [];

    List<FieldDTO> fieldDTOs =
      await _dbContext
        .Fields.Where(f => classroomDTOs.Select(c => c.Key).Contains(f.ClassroomKey))
        .ToListAsync(cancellationToken) ?? [];

    List<ConfigurationDTO> configurationDTOs =
      await _dbContext
        .Configurations.Where(co => classroomDTOs.Select(cl => cl.Key).Contains(co.ClassroomKey))
        .ToListAsync(cancellationToken) ?? [];

    List<GroupDTO> groupDTOs =
      await _dbContext
        .Groups.Where(g => configurationDTOs.Select(c => c.Key).Contains(g.ConfigurationKey))
        .ToListAsync(cancellationToken) ?? [];

    List<ColumnDTO> columnDTOs =
      await _dbContext
        .Columns.Where(col =>
          configurationDTOs.Select(con => con.Key).Contains(col.ConfigurationKey)
        )
        .ToListAsync(cancellationToken) ?? [];

    List<StudentGroupDTO> studentGroupDTOs =
      await _dbContext
        .StudentGroups.Where(sg => studentDTOs.Select(s => s.Key).Contains(sg.StudentKey))
        .ToListAsync(cancellationToken) ?? [];

    List<StudentFieldDTO> studentFieldDTOs =
      await _dbContext
        .StudentFields.Where(sg => studentDTOs.Select(s => s.Key).Contains(sg.StudentKey))
        .ToListAsync(cancellationToken) ?? [];

    var classrooms = classroomDTOs.Select(c => c.ToClassroom()).ToList();
    var students = studentDTOs.Select(s => s.ToStudent()).ToList();
    var fields = fieldDTOs.Select(f => f.ToField()).ToList();
    var columns = columnDTOs.Select(c => c.ToColumn()).ToList();
    var groups = groupDTOs.Select(g => g.ToGroup()).ToList();
    var configurations = configurationDTOs.Select(c => c.ToConfiguration()).ToList();
    var studentFields = studentFieldDTOs.Select(sf => sf.ToStudentField()).ToList();
    var studentGroups = studentGroupDTOs.Select(sg => sg.ToStudentGroup()).ToList();

    return new ClassroomDetails(
      classrooms,
      students,
      fields,
      columns,
      studentGroups,
      studentFields,
      groups,
      configurations
    );
  }
}
