using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms.Shared;

public interface IOrdinalService
{
  public Task<List<GroupDetail>> RecalculateStudentOrdinals(
    Guid accountId,
    Guid classroomId,
    List<Guid> groupIds,
    CancellationToken cancellationToken
  );
}

public class OrdinalService(ClassroomGroupsContext dbContext, IDetailService detailService)
  : IOrdinalService
{
  readonly ClassroomGroupsContext _dbContext = dbContext;

  readonly IDetailService _detailService = detailService;

  public async Task<List<GroupDetail>> RecalculateStudentOrdinals(
    Guid accountId,
    Guid classroomId,
    List<Guid> groupIds,
    CancellationToken cancellationToken
  )
  {
    var studentGroups =
      await _dbContext
        .StudentGroups.Where(sg => groupIds.Contains(sg.GroupId))
        .ToListAsync(cancellationToken) ?? throw new Exception();

    var studentGroupIds = studentGroups.Select(sg => sg.Id);

    var students = _dbContext.Students.Where(s => studentGroupIds.Contains(s.Id));

    var groups =
      await _dbContext.Groups.Where(g => groupIds.Contains(g.Id)).ToListAsync(cancellationToken)
      ?? throw new Exception();

    foreach (var group in groups)
    {
      var releventStudentGroups = studentGroups
        .Where(sg => sg.GroupId == group.Id)
        .ToList()
        .OrderBy(sg => sg.Ordinal)
        .Select(
          (sg, i) =>
          {
            sg.Ordinal = i;
            return sg;
          }
        );
      _dbContext.StudentGroups.UpdateRange(releventStudentGroups);
    }

    await _dbContext.SaveChangesAsync();

    return await _detailService.GetGroupDetails(
      accountId,
      classroomId,
      groupIds,
      cancellationToken
    );
  }
}
