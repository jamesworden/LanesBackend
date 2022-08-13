using LanesBackend.CacheModels;
using Microsoft.AspNetCore.SignalR;

namespace LanesBackend.Hubs
{
    public class GameHub : Hub
    {
        private readonly Dictionary<string, string> HostConnectionIdToPendingGameCodes = new();
        
        private readonly List<GameCacheModel> Games = new();

        //private readonly ICacheService cacheService;

        //public GameHub(ICacheService cacheService)
        //{
        //    this.cacheService = cacheService;
        //}

        public async Task CreateGame()
        {
            string gameCode = Guid.NewGuid().ToString()[..4];
            string connectionId = Context.ConnectionId;
            HostConnectionIdToPendingGameCodes.Add(connectionId, gameCode);
            await Groups.AddToGroupAsync(connectionId, gameCode);
            await Clients.Client(connectionId).SendAsync("CreatedPendingGame", gameCode);
        }

        public async Task OnDisconnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var pendingGameExists = HostConnectionIdToPendingGameCodes.ContainsKey(connectionId);
            
            if (pendingGameExists)
            {
                HostConnectionIdToPendingGameCodes.Remove(connectionId);
                return;
            }

            foreach (var game in Games)
            {
                if (game.HostConnectionId == connectionId)
                {
                    await Clients.Client(game.JoinConnectionId).SendAsync("OpponentDisconnected");
                    Games.Remove(game);
                }

                if (game.JoinConnectionId == connectionId)
                {
                    await Clients.Client(game.HostConnectionId).SendAsync("OpponentDisconnected");
                    Games.Remove(game);
                }
            }
        }
    }
}