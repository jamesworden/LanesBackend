namespace LanesBackend.Models
{
  public class ChatMessage
  {
    public string RawMessage { get; set; }

    public string SensoredMessage { get; set; }

    public DateTime SentAtUtc { get; set; }

    public PlayerOrNone SentBy { get; set; }

    public ChatMessage(
      string rawMessage,
      string sensoredMessage,
      DateTime sentAtUTC,
      PlayerOrNone sentBy
    )
    {
      RawMessage = rawMessage;
      SensoredMessage = sensoredMessage;
      SentAtUtc = sentAtUTC;
      SentBy = sentBy;
    }
  }
}
