using ClassroomGroups.Domain.Features.Classrooms.Entities;
using MediatR;

namespace ClassroomGroups.Application.Features.Accounts.Requests;

public record GetAccountRequest() : IRequest<Account?> { }
