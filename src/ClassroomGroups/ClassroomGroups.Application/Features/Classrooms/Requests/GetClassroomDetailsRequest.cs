﻿using ClassroomGroups.Application.Features.Classrooms.Responses;
using MediatR;

namespace ClassroomGroups.Application.Features.Classrooms.Requests;

public record GetClassroomDetailRequest() : IRequest<GetClassroomDetailsResponse?> { }