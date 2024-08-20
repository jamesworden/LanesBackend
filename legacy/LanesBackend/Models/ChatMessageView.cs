namespace LanesBackend.Models
{
  public class ChatMessageView
  {
    public DateTime SentAtUTC { get; set; }

    public PlayerOrNone SentBy { get; set; }

    public string Message { get; set; }

    public ChatMessageView(string message, PlayerOrNone sentBy, DateTime sentAtUTC)
    {
      Message = message;
      SentBy = sentBy;
      SentAtUTC = sentAtUTC;
    }
  }
}
