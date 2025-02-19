using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record DeleteColumnRequest(Guid ClassroomId, Guid ConfigurationId, Guid ColumnId)
  : IRequest<DeleteColumnResponse>,
    IRequiredUserAccount
{
  public EntityIds GetEntityIds() =>
    new()
    {
      ClassroomIds = [ClassroomId],
      ConfigurationIds = [ConfigurationId],
      ColumnIds = [ColumnId]
    };
}

public record DeleteColumnResponse(
  Column DeletedColumn,
  Field DeletedField,
  Dictionary<Guid, List<ColumnDetail>> ConfigurationIdsColumnDetails
);

public class DeleteColumnRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<DeleteColumnRequest, DeleteColumnResponse>
{
  public async Task<DeleteColumnResponse> Handle(
    DeleteColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var columnDTO =
        await dbContext
          .Columns.Include(c => c.ConfigurationDTO)
          .Where(c =>
            c.Id == request.ColumnId && c.ConfigurationDTO.ClassroomId == request.ClassroomId
          )
          .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      var deletedColumn = columnDTO.ToColumn();

      var fieldDTO =
        await dbContext
          .Fields.Where(f => columnDTO.FieldId == f.Id)
          .SingleOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      var deletedField = fieldDTO.ToField();

      dbContext.Columns.Remove(columnDTO);
      dbContext.Fields.Remove(fieldDTO);

      await dbContext.SaveChangesAsync(cancellationToken);

      var configurations = await dbContext
        .Configurations.Where(c => c.ClassroomId == request.ClassroomId)
        .ToListAsync();

      var ConfigurationIdsColumnDetails = new Dictionary<Guid, List<ColumnDetail>>();

      var tasks = configurations
        .Select(async configuration =>
        {
          var columnDetails = await detailService.GetColumnDetails(
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
