using LanesBackend.Models;

namespace LanesBackend.Interfaces;

public interface IPendingGameService
{
  public PendingGame CreatePendingGame(
    string hostConnectionId,
    PendingGameOptions? pendingGameOptions
  );

  public Game JoinPendingGame(
    string gameCode,
    string guestConnectionId,
    JoinPendingGameOptions? joinPendingGameOptions
  );

  public PendingGame SelectDurationOption(string connectionId, DurationOption durationOption);

  public PendingGame? RemovePendingGame(string connectionId);
}
