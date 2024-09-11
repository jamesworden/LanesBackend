using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;
using MediatR;

namespace ClassroomGroups.Application.Features.Authentication.Requests;

public record GetAccountRequest() : IRequest<AccountView> { }
