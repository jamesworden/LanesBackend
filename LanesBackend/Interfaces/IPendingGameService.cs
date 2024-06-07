using LanesBackend.Models;
using Results;

namespace LanesBackend.Interfaces;

public interface IPendingGameService
{
  public (PendingGame?, IEnumerable<CreatePendingGameResults>) CreatePendingGame(
    string hostConnectionId,
    PendingGameOptions? pendingGameOptions
  );

  public Game JoinPendingGame(
    string gameCode,
    string guestConnectionId,
    JoinPendingGameOptions? joinPendingGameOptions
  );

  public PendingGame? RemovePendingGame(string connectionId);
}
