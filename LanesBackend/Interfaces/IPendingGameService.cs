using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
  public interface IPendingGameService
  {
    public PendingGame CreatePendingGame(string hostConnectionId);

    public Game JoinPendingGame(string gameCode, string guestConnectionId);

    public PendingGame SelectDurationOption(string connectionId, DurationOption durationOption);

    public PendingGame? RemovePendingGame(string connectionId);
  }
}
