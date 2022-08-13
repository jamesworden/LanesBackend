namespace LanesBackend.CacheModels
{
    public class GameCacheModel
    {
        public GameState State = GameState.Pending;

        public string HostConnectionId { get; set; }

        public string JoinConnectionId { get; set; }
    }
}
