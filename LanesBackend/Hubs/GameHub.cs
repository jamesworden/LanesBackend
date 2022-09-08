using LanesBackend.CacheModels;
using LanesBackend.Interfaces;
using LanesBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LanesBackend.Hubs
{
    public class GameHub : Hub
    {
        private static readonly List<Game> Games = new();

        private readonly IPendingGameCache PendingGameCache;

        public GameHub(IPendingGameCache pendingGameCache)
        { 
            PendingGameCache = pendingGameCache;
        }

        public async Task CreateGame()
        {
            string gameCode = Guid.NewGuid().ToString()[..4].ToUpper();
            string hostConnectionId = Context.ConnectionId;

            var pendingGame = new PendingGame(gameCode, hostConnectionId);

            PendingGameCache.AddPendingGame(pendingGame);

            await Clients.Client(hostConnectionId).SendAsync("CreatedPendingGame", gameCode);
        }

        public async Task JoinGame(string gameCode)
        {
            var guestConnectionId = Context.ConnectionId;
            var pendingGame = PendingGameCache.GetPendingGameByGameCode(gameCode);

            if (pendingGame is null)
            {
                await Clients.Client(guestConnectionId).SendAsync("InvalidGameCode");
                return;
            }

            Game game = new(pendingGame.HostConnectionId, guestConnectionId, gameCode);
            Games.Add(game);
            PendingGameCache.RemovePendingGame(gameCode);

            await UpdatePlayerGameStates(game, "GameStarted");
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
            bool newHandHasSameCards = existingCards.Except(cards).Any() && cards.Except(existingCards).Any();

            if (!newHandHasSameCards)
            {
                return;
            }

            if (playerIsHost)
            {
                game.HostPlayer.Hand.Cards = cards;
                // await UpdateHostGameState(game, "GameUpdated");
            }
            else
            {
                game.GuestPlayer.Hand.Cards = cards;
                // await UpdateGuestGameState(game, "GameUpdated");
            }
        }

        public async Task MakeMove(string stringifiedMove)
        {
            var move = JsonConvert.DeserializeObject<Move>(stringifiedMove);

            if (move == null)
            {
                return;
            }

            var connectionId = Context.ConnectionId;
            var game = Games.Where(game => game.HostConnectionId == connectionId || game.GuestConnectionId == connectionId).FirstOrDefault();

            if (game == null)
            {
                return;
            }

            var moveWasValid = game.MakeMoveIfValid(move, connectionId);
            
            if (!moveWasValid)
            {
                return;
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            var playersDeckHasCards = player.Deck.Cards.Any();

            if (playersDeckHasCards)
            {
                var cardFromDeck = player.Deck.DrawCard();

                if (cardFromDeck != null)
                {
                    player.Hand.AddCard(cardFromDeck);
                }
            }

            player.Hand.RemoveCard(move.PlaceCardAttempts[0].Card); // For now assume all moves are one place card attempt.

            await UpdatePlayerGameStates(game, "GameUpdated");
        }

        public async override Task OnDisconnectedAsync(Exception? _)
        {
            var connectionId = Context.ConnectionId;
            var pendingGame = PendingGameCache.GetPendingGameByConnectionId(connectionId);

            if (pendingGame is not null)
            {
                PendingGameCache.RemovePendingGame(pendingGame.GameCode);
                return;
            }

            foreach (var game in Games)
            {
                var playerIsHost = game.HostConnectionId == connectionId;
                if (playerIsHost)
                {
                    await Clients.Client(game.GuestConnectionId).SendAsync("GameOver", "Opponent Disconnected.");
                    Games.Remove(game);
                }

                var playerIsGuest = game.GuestConnectionId == connectionId;
                if (playerIsGuest)
                {
                    await Clients.Client(game.HostConnectionId).SendAsync("GameOver", "Opponent Disconnected.");
                    Games.Remove(game);
                }
            }
        }

        private async Task UpdateHostGameState(Game game, string messageType)
        {
            var hostGameState = new PlayerGameState(
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand.Cards.Count,
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand,
                game.Lanes,
                true,
                game.IsHostPlayersTurn
                );

            var serializedHostGameState = JsonConvert.SerializeObject(hostGameState, new StringEnumConverter());

            await Clients.Client(game.HostConnectionId).SendAsync(messageType, serializedHostGameState);
        }

        private async Task UpdateGuestGameState(Game game, string messageType)
        {
            var guestGameState = new PlayerGameState(
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand.Cards.Count,
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand,
                game.Lanes,
                false,
                game.IsHostPlayersTurn
                );

            var serializedGuestGameState = JsonConvert.SerializeObject(guestGameState, new StringEnumConverter());
            
            await Clients.Client(game.GuestConnectionId).SendAsync(messageType, serializedGuestGameState);
        }

        private async Task UpdatePlayerGameStates(Game game, string messageType)
        {
            await UpdateHostGameState(game, messageType);
            await UpdateGuestGameState(game, messageType);
        }
    }
}