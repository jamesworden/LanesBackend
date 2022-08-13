using LanesBackend.CacheModels;
using Microsoft.AspNetCore.SignalR;

namespace LanesBackend.Hubs
{
    public class GameHub : Hub
    {
        private readonly Dictionary<string, string> HostConnectionIdToPendingGameCodes = new();
        
        private readonly List<GameCacheModel> Games = new();

        public async Task CreateGame()
        {
            string gameCode = Guid.NewGuid().ToString()[..4].ToUpper();
            string connectionId = Context.ConnectionId;
            HostConnectionIdToPendingGameCodes.Add(connectionId, gameCode);
            await Groups.AddToGroupAsync(connectionId, gameCode);
            await Clients.Client(connectionId).SendAsync("CreatedPendingGame", gameCode);
        }

        public async Task JoinGame(string gameCode)
        {
            var guestConnectionId = Context.ConnectionId;
            HostConnectionIdToPendingGameCodes.TryGetValue(gameCode, out var hostConnectionId);
 
            if (hostConnectionId is null)
            {
                await Clients.Client(guestConnectionId).SendAsync("InvalidGameCode");
                return;
            }

            await AddPlayersToRoom(hostConnectionId, guestConnectionId, gameCode);
            GameCacheModel gameCacheModel = new(hostConnectionId, guestConnectionId, gameCode);
            Games.Add(gameCacheModel);
            await Clients.Group(gameCode).SendAsync("GameStarted");
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
                var playerIsHost = game.HostConnectionId == connectionId;
                if (playerIsHost)
                {
                    await Clients.Client(game.GuestConnectionId).SendAsync("OpponentDisconnected");
                    await RemovePlayersFromRoom(game.HostConnectionId, game.GuestConnectionId, game.GameCode);
                    Games.Remove(game);
                }

                var playerIsGuest = game.GuestConnectionId == connectionId;
                if (playerIsGuest)
                {
                    await Clients.Client(game.HostConnectionId).SendAsync("OpponentDisconnected");
                    await RemovePlayersFromRoom(game.HostConnectionId, game.GuestConnectionId, game.GameCode);
                    Games.Remove(game);
                }
            }
        }

        private async Task AddPlayersToRoom(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            await Groups.AddToGroupAsync(guestConnectionId, gameCode);
            await Groups.AddToGroupAsync(hostConnectionId, gameCode);
        }

        private async Task RemovePlayersFromRoom(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            await Groups.RemoveFromGroupAsync(hostConnectionId, gameCode);
            await Groups.RemoveFromGroupAsync(guestConnectionId, gameCode);
        }
    }
}