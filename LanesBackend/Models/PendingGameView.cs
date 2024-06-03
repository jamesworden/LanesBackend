namespace LanesBackend.Models
{
  public class PendingGameView(string gameCode, DurationOption durationOption, string? hostName)
  {
    public string GameCode { get; set; } = gameCode;

    public DurationOption DurationOption { get; set; } = durationOption;

    public string? Hostname { get; set; } = hostName;
  }
}
