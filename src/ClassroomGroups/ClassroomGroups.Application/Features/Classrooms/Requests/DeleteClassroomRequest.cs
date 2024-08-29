using ClassroomGroups.Application.Features.Classrooms.Responses;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms.Requests;

public record DeleteClassroomRequest(Guid ClassroomId) : IRequest<DeleteClassroomResponse?>
{
  public Guid ClassroomId { get; set; } = ClassroomId;
}
