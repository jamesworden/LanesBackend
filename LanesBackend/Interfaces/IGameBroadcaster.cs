using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
  public interface IGameBroadcaster
  {
    public Task BroadcastPlayerGameViews(Game game, string messageType);
  }
}
