using MediatR;

namespace ChessOfCards.Application.Features.Games;

public record AcceptDrawOfferCommand(string ConnectionId) : INotification
{
  public string ConnectionId { get; } = ConnectionId;
}
