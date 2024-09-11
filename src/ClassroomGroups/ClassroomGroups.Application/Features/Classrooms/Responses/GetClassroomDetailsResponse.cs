using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.Application.Features.Classrooms.Responses;

public record GetClassroomDetailsResponse(List<ClassroomDetail> ClassroomDetails) { }
