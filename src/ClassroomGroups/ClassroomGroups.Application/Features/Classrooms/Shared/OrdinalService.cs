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
  public async Task<List<GroupDetail>> RecalculateStudentOrdinals(
    Guid accountId,
    Guid classroomId,
    List<Guid> groupIds,
    CancellationToken cancellationToken
  )
  {
    var studentGroups =
      await dbContext
        .StudentGroups.Where(sg => groupIds.Contains(sg.GroupId))
        .ToListAsync(cancellationToken) ?? throw new InvalidOperationException();

    var studentGroupIds = studentGroups.Select(sg => sg.Id);

    var students = dbContext.Students.Where(s => studentGroupIds.Contains(s.Id));

    var groups =
      await dbContext.Groups.Where(g => groupIds.Contains(g.Id)).ToListAsync(cancellationToken)
      ?? throw new InvalidOperationException();

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
      dbContext.StudentGroups.UpdateRange(releventStudentGroups);
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    return await detailService.GetGroupDetails(accountId, classroomId, groupIds, cancellationToken);
  }
}
