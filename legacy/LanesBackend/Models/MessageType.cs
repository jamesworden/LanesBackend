namespace LanesBackend.Models
{
  public static class MessageType
  {
    public static readonly string CreatedPendingGame = "CreatedPendingGame";

    public static readonly string InvalidGameCode = "InvalidGameCode";

    public static readonly string GameStarted = "GameStarted";

    public static readonly string GameUpdated = "GameUpdated";

    public static readonly string GameOver = "GameOver";

    public static readonly string PassedMove = "PassedMove";

    public static readonly string DrawOffered = "DrawOffered";

    public static readonly string PendingGameUpdated = "PendingGameUpdated";

    public static readonly string TurnSkippedNoMoves = "TurnSkippedNoMoves";

    public static readonly string NewChatMessage = "NewChatMessage";

    public static readonly string InvalidName = "InvalidName";

    public static readonly string OpponentDisconnected = "OpponentDisconnected";

    public static readonly string OpponentReconnected = "OpponentReconnected";

    public static readonly string Reconnected = "Reconnected";
  }
}
