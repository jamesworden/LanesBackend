namespace LanesBackend.CacheModels
{
    public class GameState
    {
        public bool isRunning = false;

        public string HostConnectionId { get; set; }

        public string GuestConnectionId { get; set; }

        public string GameCode { get; set; }

        public GameState(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            HostConnectionId = hostConnectionId;
            GuestConnectionId = guestConnectionId;
            GameCode = gameCode;
        }

    }
}
