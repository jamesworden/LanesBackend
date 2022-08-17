﻿using LanesBackend.CacheModels;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LanesBackend.Hubs
{
    public class GameHub : Hub
    {
        private static readonly Dictionary<string, string> PendingGameCodeToHostConnectionId = new();
        
        private static readonly List<Game> Games = new();

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
            Game game = new(hostConnectionId, guestConnectionId, gameCode);
            Games.Add(game);
            PendingGameCodeToHostConnectionId.Remove(gameCode);
            await Clients.Group(gameCode).SendAsync("GameStarted", JsonConvert.SerializeObject(game, new StringEnumConverter()));
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
                    await Clients.Client(game.GuestConnectionId).SendAsync("GameOver", "Opponent Disconnected.");
                    await RemovePlayersFromRoom(game.HostConnectionId, game.GuestConnectionId, game.GameCode);
                    Games.Remove(game);
                }

                var playerIsGuest = game.GuestConnectionId == connectionId;
                if (playerIsGuest)
                {
                    await Clients.Client(game.HostConnectionId).SendAsync("GameOver", "Opponent Disconnected.");
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