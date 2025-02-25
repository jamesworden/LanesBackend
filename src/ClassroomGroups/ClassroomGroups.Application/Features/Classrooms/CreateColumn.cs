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
  FieldType Type,
  int? Ordinal
) : IRequest<CreateColumnResponse>, IRequiredUserAccount
{
  public EntityIds GetEntityIds() =>
    new() { ClassroomIds = [ClassroomId], ConfigurationIds = [ConfigurationId] };
}

public record CreateColumnResponse(
  List<ColumnDetail> UpdatedColumnDetails,
  List<FieldDetail> UpdatedFieldDetails
) { }

public class CreateColumnRequestHandler(
  ClassroomGroupsContext dbContext,
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService
) : IRequestHandler<CreateColumnRequest, CreateColumnResponse>
{
  public async Task<CreateColumnResponse> Handle(
    CreateColumnRequest request,
    CancellationToken cancellationToken
  )
  {
    var account = authBehaviorCache.Account;

    var existingFieldDTOs = await dbContext
      .Fields.Where(c => c.ClassroomId == request.ClassroomId)
      .ToListAsync(cancellationToken);

    if (existingFieldDTOs.Count >= account.Subscription.MaxFieldsPerClassroom)
    {
      throw new InvalidOperationException("Maximum fields per classroom exceeded.");
    }

    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var classroomDTO =
        await dbContext
          .Classrooms.Where(c => c.Id == request.ClassroomId)
          .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException();

      var configurationDTOs =
        await dbContext.Configurations.ToListAsync(cancellationToken)
        ?? throw new InvalidOperationException();

      var fieldDTO = new FieldDTO()
      {
        Id = Guid.NewGuid(),
        ClassroomId = classroomDTO.Id,
        ClassroomKey = classroomDTO.Key,
        Label = request.Label,
        Type = request.Type
      };

      dbContext.Fields.Add(fieldDTO);

      await dbContext.SaveChangesAsync(cancellationToken);

      foreach (var config in configurationDTOs)
      {
        if (config.Id == request.ConfigurationId)
        {
          var highestOrdinal =
            await dbContext
              .Columns.Where(c => c.ConfigurationId == config.Id)
              .MaxAsync(c => (int?)c.Ordinal, cancellationToken) ?? 0;

          var newColumn = new ColumnDTO()
          {
            Id = Guid.NewGuid(),
            ConfigurationId = config.Id,
            ConfigurationKey = config.Key,
            FieldId = fieldDTO.Id,
            FieldKey = fieldDTO.Key,
            Ordinal = request.Ordinal ?? highestOrdinal + 1,
            Sort = ColumnSort.NONE,
            Enabled = true
          };

          dbContext.Columns.Add(newColumn);

          if (request.Ordinal.HasValue)
          {
            var columnsToReorder = await dbContext
              .Columns.Where(c =>
                c.ConfigurationId == request.ConfigurationId && c.Ordinal >= request.Ordinal.Value
              )
              .ToListAsync(cancellationToken);

            foreach (var column in columnsToReorder)
            {
              column.Ordinal++;
            }
          }
        }
        else
        {
          var highestOrdinal =
            await dbContext
              .Columns.Where(c => c.ConfigurationId == config.Id)
              .MaxAsync(c => (int?)c.Ordinal, cancellationToken) ?? 0;

          var newColumn = new ColumnDTO()
          {
            Id = Guid.NewGuid(),
            ConfigurationId = config.Id,
            ConfigurationKey = config.Key,
            FieldId = fieldDTO.Id,
            FieldKey = fieldDTO.Key,
            Ordinal = highestOrdinal + 1,
            Sort = ColumnSort.NONE,
            Enabled = true
          };

          dbContext.Columns.Add(newColumn);
        }
      }

      await dbContext.SaveChangesAsync(cancellationToken);

      var columns = await dbContext
        .Columns.Where(c => c.ConfigurationId == request.ConfigurationId)
        .ToListAsync(cancellationToken);

      var fields = await dbContext
        .Fields.Where(f => f.ClassroomId == request.ClassroomId)
        .ToListAsync(cancellationToken);

      await transaction.CommitAsync(cancellationToken);

      var columnDetails = await detailService.GetColumnDetails(
        account.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      );

      var fieldDetails =
        (
          await dbContext
            .Fields.Where(f => f.ClassroomId == request.ClassroomId)
            .Select(f => new FieldDetailDTO(f.Id, f.ClassroomId, f.Label, f.Type))
            .ToListAsync(cancellationToken)
        )
          .Select(f => f.ToFieldDetail())
          .ToList() ?? [];

      return new CreateColumnResponse(columnDetails, fieldDetails);
    }
    catch (Exception)
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
