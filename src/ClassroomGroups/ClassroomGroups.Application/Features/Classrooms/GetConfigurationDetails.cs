using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record GetConfigurationDetailResponse(ConfigurationDetail ConfigurationDetail) { }

public record GetConfigurationDetailRequest(Guid ClassroomId, Guid ConfigurationId)
  : IRequest<GetConfigurationDetailResponse> { }

public class GetConfigurationDetailRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache
) : IRequestHandler<GetConfigurationDetailRequest, GetConfigurationDetailResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  public async Task<GetConfigurationDetailResponse> Handle(
    GetConfigurationDetailRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

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

    var configurationDetail =
      (
        await _dbContext
          .Configurations.Where(c =>
            c.Id == request.ConfigurationId && c.ClassroomId == request.ClassroomId
          )
          .Select(c => new ConfigurationDetailDTO(c.Id, c.ClassroomId, c.Label, c.Description))
          .FirstOrDefaultAsync(cancellationToken)
      )?.ToConfigurationDetail(groupDetails, columnDetails) ?? throw new Exception();

    return new GetConfigurationDetailResponse(configurationDetail);
  }
}
