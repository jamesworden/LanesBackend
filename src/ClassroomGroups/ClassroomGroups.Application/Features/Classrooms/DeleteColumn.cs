using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteColumnRequest(Guid ClassroomId, Guid ConfigurationId, Guid ColumnId)
  : IRequest<DeleteColumnResponse>;

public record DeleteColumnResponse(
  Column DeletedColumn,
  Field DeletedField,
  Dictionary<Guid, List<ColumnDetail>> ConfigurationIdsColumnDetails
);

public class DeleteColumnRequestHandler(
  ClassroomGroupsContext dbContext,
  AuthBehaviorCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<DeleteColumnRequest, DeleteColumnResponse>
{
  readonly ClassroomGroupsContext _dbContext = dbContext;
  readonly AuthBehaviorCache _authBehaviorCache = authBehaviorCache;
  readonly IDetailService _detailService = detailService;

  public async Task<DeleteColumnResponse> Handle(
    DeleteColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = _authBehaviorCache.Account ?? throw new Exception();

    await using var transaction = await _dbContext.Database.BeginTransactionAsync(
      cancellationToken
    );

    try
    {
      var columnDTO =
        await _dbContext
          .Columns.Include(c => c.ConfigurationDTO)
          .Where(c =>
            c.Id == request.ColumnId && c.ConfigurationDTO.ClassroomId == request.ClassroomId
          )
          .SingleOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var deletedColumn = columnDTO.ToColumn();

      var fieldDTO =
        await _dbContext
          .Fields.Where(f => columnDTO.FieldId == f.Id)
          .SingleOrDefaultAsync(cancellationToken) ?? throw new Exception();

      var deletedField = fieldDTO.ToField();

      _dbContext.Columns.Remove(columnDTO);
      _dbContext.Fields.Remove(fieldDTO);

      await _dbContext.SaveChangesAsync(cancellationToken);

      var configurations = await _dbContext
        .Configurations.Where(c => c.ClassroomId == request.ClassroomId)
        .ToListAsync();

      var ConfigurationIdsColumnDetails = new Dictionary<Guid, List<ColumnDetail>>();

      var tasks = configurations
        .Select(async configuration =>
        {
          var columnDetails = await _detailService.GetColumnDetails(
            account.Id,
            request.ClassroomId,
            configuration.Id,
            cancellationToken
          );
          ConfigurationIdsColumnDetails[configuration.Id] = columnDetails;
        })
        .ToArray();

      await Task.WhenAll(tasks);

      await transaction.CommitAsync(cancellationToken);

      return new DeleteColumnResponse(deletedColumn, deletedField, ConfigurationIdsColumnDetails);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
