using LanesBackend.Models;

namespace LanesBackend.Interfaces;

public interface IPendingGameCache
{
  public void AddPendingGame(PendingGame pendingGame);

  public PendingGame? GetPendingGameByGameCode(string gameCode);

  public PendingGame? GetPendingGameByConnectionId(string hostConnectionId);

  public bool RemovePendingGame(string gameCode);
}
