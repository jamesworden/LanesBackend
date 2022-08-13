namespace LanesBackend.CacheModels
{
    public class GameCacheModel
    {
        public bool isRunning = false;

        public string HostConnectionId { get; set; }

        public string GuestConnectionId { get; set; }

        public string GameCode { get; set; }

        public GameCacheModel(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            HostConnectionId = hostConnectionId;
            GuestConnectionId = guestConnectionId;
            GameCode = gameCode;
        }

    }
}
