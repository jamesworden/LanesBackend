using LanesBackend.CacheModels;
using LanesBackend.Models;
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

            var hostGameState = new PlayerGameState(
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand.Cards.Count,
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand,
                game.Lanes,
                true
                );

            var guestGameState = new PlayerGameState(
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand.Cards.Count,
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand,
                game.Lanes,
                false
                );

            var serializedHostGameState = JsonConvert.SerializeObject(hostGameState, new StringEnumConverter());
            var serializedGuestGameState = JsonConvert.SerializeObject(guestGameState, new StringEnumConverter());

            await Clients.Client(hostConnectionId).SendAsync("GameStarted", serializedHostGameState);
            await Clients.Client(guestConnectionId).SendAsync("GameStarted", serializedGuestGameState);
        }

        public async Task RearrangeHand(string stringifiedCards)
        {
            var cards = JsonConvert.DeserializeObject<List<Card>>(stringifiedCards);

            if (cards == null)
            {
                return;
            }

            var connectionId = Context.ConnectionId;
            var game = Games.Where(game => game.HostConnectionId == connectionId || game.GuestConnectionId == connectionId).FirstOrDefault();

            if (game == null)
            {
                return;
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            var existingCards = playerIsHost ? game.HostPlayer.Hand.Cards : game.GuestPlayer.Hand.Cards;
            bool newHandHasSameCards = existingCards.ToHashSet().SetEquals(cards);
            
            if (!newHandHasSameCards)
            {
                return;
            }

            if (playerIsHost)
            {
                game.HostPlayer.Hand.Cards = cards;
            }
            else
            {
                game.GuestPlayer.Hand.Cards = cards;
            }

            var hostGameState = new PlayerGameState(
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand.Cards.Count,
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand,
                game.Lanes,
                true
                );

            var guestGameState = new PlayerGameState(
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand.Cards.Count,
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand,
                game.Lanes,
                false
                );

            var serializedHostGameState = JsonConvert.SerializeObject(hostGameState, new StringEnumConverter());
            var serializedGuestGameState = JsonConvert.SerializeObject(guestGameState, new StringEnumConverter());

            await Clients.Client(game.HostConnectionId).SendAsync("GameUpdated", serializedHostGameState);
            await Clients.Client(game.GuestConnectionId).SendAsync("GameUpdated", serializedGuestGameState);
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