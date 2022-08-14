using LanesBackend.CacheModels;
using Microsoft.AspNetCore.SignalR;

namespace LanesBackend.Hubs
{
    public class GameHub : Hub
    {
        private static readonly Dictionary<string, string> PendingGameCodeToHostConnectionId = new();
        
        private static readonly List<GameState> Games = new();

        public async Task CreateGame()
        {
            string gameCode = Guid.NewGuid().ToString()[..4].ToUpper();
            string connectionId = Context.ConnectionId;
            PendingGameCodeToHostConnectionId.Add(gameCode, connectionId);
            await Groups.AddToGroupAsync(connectionId, gameCode);
            await Clients.Client(connectionId).SendAsync("CreatedPendingGame", gameCode);
        }

        public async Task JoinGame(string gameCode)
        {
            var guestConnectionId = Context.ConnectionId;
            PendingGameCodeToHostConnectionId.TryGetValue(gameCode, out var hostConnectionId);
 
            if (hostConnectionId is null)
            {
                await Clients.Client(guestConnectionId).SendAsync("InvalidGameCode");
                return;
            }

            await AddPlayersToRoom(hostConnectionId, guestConnectionId, gameCode);
            GameState gameCacheModel = new(hostConnectionId, guestConnectionId, gameCode);
            Games.Add(gameCacheModel);
            PendingGameCodeToHostConnectionId.Remove(gameCode);
            await Clients.Group(gameCode).SendAsync("GameStarted");
        }

        public async override Task OnDisconnectedAsync(Exception? _)
        {
            var connectionId = Context.ConnectionId;
            var pendingGameCode = PendingGameCodeToHostConnectionId
                .FirstOrDefault(row => row.Value == connectionId).Key;

            if (pendingGameCode is not null)
            {
                PendingGameCodeToHostConnectionId.Remove(pendingGameCode);
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