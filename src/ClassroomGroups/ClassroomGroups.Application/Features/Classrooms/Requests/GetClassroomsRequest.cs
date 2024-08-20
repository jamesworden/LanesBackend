using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms.Requests;

public record GetClassroomsRequest() : IRequest<IEnumerable<Classroom>> { }
