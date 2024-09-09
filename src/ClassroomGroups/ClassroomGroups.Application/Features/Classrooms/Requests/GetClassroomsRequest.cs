using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms.Requests;

public record GetClassroomsRequest(Guid ConfigurationId) : IRequest<List<Classroom>?> { }
