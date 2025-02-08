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
) : IRequest<CreateColumnResponse>, IRequiredUserAccount { }

public record CreateColumnResponse(
  ColumnDetail CreatedColumnDetail,
  FieldDetail CreatedFieldDetail
) { }

public class CreateColumnRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<CreateColumnRequest, CreateColumnResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly AccountRequiredCache _authBehaviorCache = authBehaviorCache;

  readonly IDetailService _detailService = detailService;

  public async Task<CreateColumnResponse> Handle(
    CreateColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account;

    var existingFieldDTOs = await _dbContext
      .Fields.Where(c => c.ClassroomId == request.ClassroomId)
      .ToListAsync(cancellationToken);

    if (existingFieldDTOs.Count >= account.Subscription.MaxFieldsPerClassroom)
    {
      throw new Exception();
    }

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var classroomDTO =
        await _dbContext
          .Classrooms.Where(c => c.Id == request.ClassroomId)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var configurationDTOs =
        await _dbContext.Configurations.ToListAsync(cancellationToken) ?? throw new Exception();

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

      var ordinal = existingFieldDTOs.Count + 1;

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

      transaction.Commit();

      return new CreateColumnResponse(columnDetail, fieldDetail);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
