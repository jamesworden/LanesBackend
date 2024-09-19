using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
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
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<CreateColumnRequest, CreateColumnResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  public async Task<CreateColumnResponse> Handle(
    CreateColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    var classroomDTO =
      await _dbContext
        .Classrooms.Where(c => c.Id == request.ClassroomId)
        .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

    var configurationDTOs = await _dbContext.Configurations.ToListAsync() ?? throw new Exception();

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

    var existingFieldDTOs = await _dbContext
      .Fields.Where(c => c.ClassroomId == request.ClassroomId)
      .ToListAsync(cancellationToken);

    var ordinal = existingFieldDTOs.Count;

    var columnId = Guid.NewGuid();

    var columnDTOs = configurationDTOs.Select(c => new ColumnDTO()
    {
      Id = columnId,
      ConfigurationId = c.Id,
      ConfigurationKey = c.Key,
      FieldId = fieldDTO.Id,
      FieldKey = fieldDTO.Key,
      Ordinal = ordinal,
      Sort = ColumnSort.NONE,
      Enabled = true,
    });

    await _dbContext.Columns.AddRangeAsync(columnDTOs, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    var fieldDetail = fieldEntity.Entity.ToField().ToFieldDetail();

    var columnDetails = await _detailService.GetColumnDetails(
      account.Id,
      request.ClassroomId,
      request.ConfigurationId,
      cancellationToken
    );

    var columnDetail =
      columnDetails.Where(c => c.Id == columnId).FirstOrDefault() ?? throw new Exception();

    return new CreateColumnResponse(columnDetail, fieldDetail);
  }
}
