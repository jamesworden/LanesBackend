using LanesBackend.Models;

namespace LanesBackend.Interfaces;

public interface IGameBroadcaster
{
  public Task BroadcastPlayerGameViews(Game game, string messageType);

  public Task BroadcastHostGameView(Game game, string messageType);

  public Task BroadcastGuestGameView(Game game, string messageType);
}
