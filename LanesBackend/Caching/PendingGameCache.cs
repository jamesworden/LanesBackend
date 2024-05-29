using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Caching
{
  public class PendingGameCache : IPendingGameCache
  {
    private static readonly Dictionary<string, PendingGame> PendingGameCodeToHostConnectionId =
      new();

    public void AddPendingGame(PendingGame pendingGame)
    {
      PendingGameCodeToHostConnectionId.Add(pendingGame.GameCode, pendingGame);
    }

    public PendingGame? GetPendingGameByGameCode(string gameCode)
    {
      PendingGameCodeToHostConnectionId.TryGetValue(gameCode, out var pendingGame);

      return pendingGame;
    }

    public PendingGame? GetPendingGameByConnectionId(string hostConnectionId)
    {
      var row = PendingGameCodeToHostConnectionId.FirstOrDefault(row =>
        row.Value is not null && row.Value.HostConnectionId == hostConnectionId
      );
      return row.Value ?? null;
    }

    public bool RemovePendingGame(string gameCode)
    {
      var pendingGameRemoved = PendingGameCodeToHostConnectionId.Remove(gameCode);

      return pendingGameRemoved;
    }
  }
}
