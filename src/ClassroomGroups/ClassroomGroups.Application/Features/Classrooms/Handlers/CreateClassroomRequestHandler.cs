using System.Security.Claims;
using ClassroomGroups.Application.Features.Accounts.Responses;
using ClassroomGroups.Application.Features.Classrooms.Requests;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.DataAccess.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Handlers;

public class CreateClassroomRequestHandler(
  ClassroomGroupsContext dbContext,
  IHttpContextAccessor httpContextAccessor
) : IRequestHandler<CreateClassroomRequest, CreateClassroomResponse?>
{
  ClassroomGroupsContext _dbContext = dbContext;

  IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task<CreateClassroomResponse?> Handle(
    CreateClassroomRequest request,
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

    var classroomDTO = new ClassroomDTO()
    {
      Id = Guid.NewGuid(),
      Label = request.Label,
      Description = "",
      AccountKey = accountDTO.Key,
      AccountId = accountDTO.Id
    };
    var classroomEntity = await _dbContext.Classrooms.AddAsync(classroomDTO, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
    var classroom = classroomEntity.Entity?.ToClassroom();
    if (classroom is null)
    {
      return null;
    }

    var configurationDTO = new ConfigurationDTO
    {
      Id = Guid.NewGuid(),
      Label = request.Label,
      ClassroomId = classroom.Id,
      ClassroomKey = classroomDTO.Key
    };
    var configurationEntity = await _dbContext.Configurations.AddAsync(
      configurationDTO,
      cancellationToken
    );
    await _dbContext.SaveChangesAsync(cancellationToken);
    var configuration = configurationEntity.Entity?.ToConfiguration();
    if (configuration is null)
    {
      return null;
    }

    // List<FieldDTO> fieldDTOs =
    //   await _dbContext
    //     .Fields.Where(f => f.ClassroomKey == classroomDTO.Key)
    //     .ToListAsync(cancellationToken) ?? [];

    // List<ColumnDTO> columnDTOs = fieldDTOs
    //   .Select(
    //     (f, index) =>
    //       new ColumnDTO()
    //       {
    //         Id = Guid.NewGuid(),
    //         Enabled = true,
    //         Ordinal = index,
    //         ConfigurationId = configuration.Id,
    //         ConfigurationKey = configurationDTO.Key,
    //         Sort = ColumnSort.NONE,
    //         FieldId = f.Id,
    //         FieldKey = f.Key
    //       }
    //   )
    //   .ToList();

    // await _dbContext.Columns.AddRangeAsync(columnDTOs, cancellationToken);
    // await _dbContext.SaveChangesAsync(cancellationToken);

    // List<ColumnDTO> resultingColumnDTOs =
    //   await _dbContext
    //     .Columns.Where(col => columnDTOs.Select(c => c.Id).Contains(col.Id))
    //     .ToListAsync(cancellationToken) ?? [];

    // var columns = columnDTOs.Select(c => c.ToColumn()).ToList();

    return new CreateClassroomResponse(classroom, configuration);
  }
}
