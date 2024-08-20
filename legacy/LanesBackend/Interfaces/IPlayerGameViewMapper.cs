using LanesBackend.Models;

namespace LanesBackend.Interfaces;

public interface IPlayerGameViewMapper
{
  public PlayerGameView MapToHostPlayerGameView(Game game);

  public PlayerGameView MapToGuestPlayerGameView(Game game);
}
