using System.Security.Claims;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.Application.Features.Classrooms.Responses;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class GetConfigurationDetailRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetConfigurationDetailRequest, GetConfigurationDetailResponse?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<GetConfigurationDetailResponse?> Handle(
    GetConfigurationDetailRequest request,
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

    var studentDetails =
      (
        await _dbContext
          .StudentGroups.Where(sg => sg.GroupDTO.ConfigurationId == request.ConfigurationId)
          .Select(sg => new StudentDetailDTO(sg.Id, sg.GroupId, sg.Ordinal))
          .ToListAsync(cancellationToken)
      )
        .Select(sg => sg.ToStudentDetail())
        .ToList() ?? [];

    var groupDetails =
      (
        await _dbContext
          .Groups.Where(g => g.ConfigurationId == request.ConfigurationId)
          .Select(g => new GroupDetailDTO(g.Id, g.ConfigurationId, g.Label, g.Ordinal))
          .ToListAsync(cancellationToken)
      )
        .Select(g => g.ToGroupDetail(studentDetails))
        .ToList() ?? [];

    var columnDetails = (
      await _dbContext
        .Columns.Where(c => c.Id == request.ConfigurationId)
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
          c.field.Type
        ))
        .ToListAsync(cancellationToken)
    )
      .Select(c => c.ToColumnDetail())
      .ToList();

    var configurationDetail = (
      await _dbContext
        .Configurations.Where(c =>
          c.Id == request.ConfigurationId && c.ClassroomId == request.ClassroomId
        )
        .Select(c => new ConfigurationDetailDTO(c.Id, c.ClassroomId, c.Label, c.Description))
        .FirstOrDefaultAsync(cancellationToken)
    )?.ToConfigurationDetail(groupDetails, columnDetails);

    if (configurationDetail is null)
    {
      return null;
    }

    return new GetConfigurationDetailResponse(configurationDetail);
  }
}
