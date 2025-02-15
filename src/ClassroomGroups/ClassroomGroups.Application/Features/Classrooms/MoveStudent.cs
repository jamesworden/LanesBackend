using ClassroomGroups.Application.Behaviors;
using ClassroomGroups.Application.Features.Classrooms.Shared;
using ClassroomGroups.DataAccess.Contexts;
using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomGroups.Application.Features.Classrooms;

public record MoveStudentDetail(
  int PrevIndex,
  Guid PrevGroupId,
  int CurrIndex,
  Guid CurrGroupId,
  Guid StudentId
) { }

public record MoveStudentRequest(
  Guid ClassroomId,
  Guid ConfigurationId,
  MoveStudentDetail MoveStudentDetail
) : IRequest<MoveStudentResponse>, IRequiredUserAccount { }

public record MoveStudentResponse(List<GroupDetail> UpdatedGroupDetails) { }

public class MoveStudentRequestHandler(
  AccountRequiredCache authBehaviorCache,
  IDetailService detailService,
  ClassroomGroupsContext dbContext
) : IRequestHandler<MoveStudentRequest, MoveStudentResponse>
{
  public async Task<MoveStudentResponse> Handle(
    MoveStudentRequest request,
    CancellationToken cancellationToken
  )
  {
    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

    try
    {
      var groupDetails = await MoveStudentBetweenGroups(request, cancellationToken);

      await dbContext.SaveChangesAsync(cancellationToken);
      await transaction.CommitAsync(cancellationToken);

      return new MoveStudentResponse(groupDetails);
    }
    catch
    {
      await transaction.RollbackAsync(cancellationToken);
      throw;
    }
  }

  private async Task<List<GroupDetail>> MoveStudentBetweenGroups(
    MoveStudentRequest request,
    CancellationToken cancellationToken
  )
  {
    var moveDetail = request.MoveStudentDetail;

    if (moveDetail.CurrGroupId == moveDetail.PrevGroupId)
    {
      await ReorderStudentsInSameGroup(moveDetail, cancellationToken);
    }
    else
    {
      await MoveStudentAcrossGroups(moveDetail, cancellationToken);
    }

    return await detailService.GetGroupDetails(
        authBehaviorCache.Account!.Id,
        request.ClassroomId,
        request.ConfigurationId,
        cancellationToken
      ) ?? throw new InvalidOperationException();
  }

  private async Task ReorderStudentsInSameGroup(
    MoveStudentDetail moveDetail,
    CancellationToken cancellationToken
  )
  {
    var studentGroups = await dbContext
      .StudentGroups.Where(sg => sg.GroupId == moveDetail.CurrGroupId)
      .OrderBy(sg => sg.Ordinal)
      .ToListAsync(cancellationToken);

    var studentGroup =
      studentGroups.FirstOrDefault(sg => sg.StudentId == moveDetail.StudentId)
      ?? throw new InvalidOperationException("Student not found in the current group");

    studentGroups.Remove(studentGroup);
    studentGroups.Insert(moveDetail.CurrIndex, studentGroup);

    for (int i = 0; i < studentGroups.Count; i++)
    {
      studentGroups[i].Ordinal = i;
    }
  }

  private async Task MoveStudentAcrossGroups(
    MoveStudentDetail moveDetail,
    CancellationToken cancellationToken
  )
  {
    var prevGroupStudentGroups = await dbContext
      .StudentGroups.Where(sg => sg.GroupId == moveDetail.PrevGroupId)
      .OrderBy(sg => sg.Ordinal)
      .ToListAsync(cancellationToken);

    var currGroupStudentGroups = await dbContext
      .StudentGroups.Where(sg => sg.GroupId == moveDetail.CurrGroupId)
      .OrderBy(sg => sg.Ordinal)
      .ToListAsync(cancellationToken);

    var studentGroup =
      prevGroupStudentGroups.FirstOrDefault(sg => sg.StudentId == moveDetail.StudentId)
      ?? throw new InvalidOperationException("Student not found in the previous group");

    studentGroup.GroupId = moveDetail.CurrGroupId;

    prevGroupStudentGroups.Remove(studentGroup);
    currGroupStudentGroups.Insert(moveDetail.CurrIndex, studentGroup);

    for (int i = 0; i < prevGroupStudentGroups.Count; i++)
    {
      prevGroupStudentGroups[i].Ordinal = i;
    }
    for (int i = 0; i < currGroupStudentGroups.Count; i++)
    {
      currGroupStudentGroups[i].Ordinal = i;
    }
  }
}
