using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;

namespace ClassroomGroups.Application.Features.Accounts.Requests;

public record UpsertAccountRequest() : IRequest<Account?> { }
