using LanesBackend.Models;

namespace LanesBackend.Interfaces;

public interface IGameBroadcaster
{
  public Task BroadcastPlayerGameViews(Game game, string messageType, string? message = null);

  public Task BroadcastHostGameView(Game game, string messageType, string? message = null);

  public Task BroadcastGuestGameView(Game game, string messageType, string? message = null);
}
