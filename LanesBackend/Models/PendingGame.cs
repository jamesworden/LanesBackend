namespace LanesBackend.Models
{
    public class PendingGame
    {
        public string GameCode { get; set; }

        public string HostConnectionId { get; set; }

        public PendingGame(string gameCode, string hostConnectionId)
        {
            GameCode = gameCode;
            HostConnectionId = hostConnectionId;
        }
    }
}
