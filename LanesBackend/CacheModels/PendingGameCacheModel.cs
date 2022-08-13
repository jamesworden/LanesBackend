namespace LanesBackend.CacheModels
{
    public class PendingGameCacheModel
    {
        public string HostConnectionId { get; set; }

        public PendingGameCacheModel(string hostConnectionId)
        {
            HostConnectionId = hostConnectionId;
        }
    }
}
