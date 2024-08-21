namespace LanesBackend.Models
{
  public class PendingGame
  {
    public string GameCode { get; set; }

    public string HostConnectionId { get; set; }

    public DurationOption DurationOption { get; set; }

    public string? HostName { get; set; }

    public PendingGame(
      string gameCode,
      string hostConnectionId,
      PendingGameOptions? pendingGameOptions
    )
    {
      GameCode = gameCode;
      HostConnectionId = hostConnectionId;

      if (pendingGameOptions is not null)
      {
        DurationOption = pendingGameOptions.DurationOption;
        HostName = pendingGameOptions.HostName;
      }
    }
  }
}
