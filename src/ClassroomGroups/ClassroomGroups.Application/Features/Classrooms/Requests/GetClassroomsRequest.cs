using ClassroomGroups.Domain.Features.Classrooms;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms.Requests;

public record GetClassroomsRequest() : IRequest<IEnumerable<Classroom>> { }
