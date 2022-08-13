namespace LanesBackend.CacheModels
{
    public class GameCacheModel
    {
        public bool isRunning = false;

        public string HostConnectionId { get; set; }

        public string JoinConnectionId { get; set; }

        public string GameCode { get; set; }

        public GameCacheModel(string hostConnectionId, string joinConnectionId, string gameCode)
        {
            HostConnectionId = hostConnectionId;
            JoinConnectionId = joinConnectionId;
            GameCode = gameCode;
        }

    }
}
