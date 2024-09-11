using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;

namespace ClassroomGroups.Application.Features.Authentication.Requests;

public record UpsertAccountRequest() : IRequest<AccountView?> { }
