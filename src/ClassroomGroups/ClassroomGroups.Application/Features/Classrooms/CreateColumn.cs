using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record CreateColumnRequest(
  Guid ClassroomId,
  Guid ConfigurationId,
  string Label,
  FieldType Type
) : IRequest<CreateColumnResponse> { }

public record CreateColumnResponse(
  ColumnDetail CreatedColumnDetail,
  FieldDetail CreatedFieldDetail
) { }

public class CreateColumnRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache
) : IRequestHandler<CreateColumnRequest, CreateColumnResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache authBehaviorCache = authBehaviorCache;

  public async Task<CreateColumnResponse> Handle(
    CreateColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account ?? throw new Exception();

    var classroomDTO =
      await _dbContext
        .Classrooms.Where(c => c.Id == request.ClassroomId)
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    var configurationDTO =
      await _dbContext
        .Configurations.Where(c => c.Id == request.ConfigurationId)
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    var fieldDTO = new FieldDTO()
    {
      Id = Guid.NewGuid(),
      ClassroomId = classroomDTO.Id,
      ClassroomKey = classroomDTO.Key,
      Label = request.Label,
      Type = request.Type
    };

    var fieldEntity = await _dbContext.Fields.AddAsync(fieldDTO, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    var existingColumnDTOs = await _dbContext
      .Columns.Where(c => c.ConfigurationId == configurationDTO.Id)
      .ToListAsync(cancellationToken);

    var ordinal = existingColumnDTOs.Count;

    var columnDTO = new ColumnDTO()
    {
      Id = Guid.NewGuid(),
      ConfigurationId = configurationDTO.Id,
      ConfigurationKey = configurationDTO.Key,
      FieldId = fieldDTO.Id,
      FieldKey = fieldDTO.Key,
      Ordinal = ordinal,
      Sort = ColumnSort.NONE,
      Enabled = true,
    };

    var columnEntity = await _dbContext.Columns.AddAsync(columnDTO, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    var fieldDetail = fieldEntity.Entity.ToField().ToFieldDetail();

    var columnDetail = columnEntity.Entity.ToColumn().ToColumnDetail(fieldDetail.Type);

    return new CreateColumnResponse(columnDetail, fieldDetail);
  }
}
