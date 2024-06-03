namespace LanesBackend.Models
{
  public class PendingGameView
  {
    public string GameCode { get; set; }

    public DurationOption DurationOption { get; set; }

    public string Hostname { get; set; }

    public PendingGameView(string gameCode, DurationOption durationOption, string hostName)
    {
      GameCode = gameCode;
      DurationOption = durationOption;
      Hostname = hostName;
    }
  }
}
