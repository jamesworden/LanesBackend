﻿using LanesBackend.Interfaces;
using LanesBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LanesBackend.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameCache GameCache;

        private readonly IPendingGameCache PendingGameCache;

        private readonly IGameService GameService;
        
        private readonly IDeckService DeckService;

        private readonly ICardService CardService;

        public GameHub(
            IGameCache gameCache, 
            IPendingGameCache pendingGameCache, 
            IGameService gameService, 
            IDeckService deckService, 
            ICardService cardService)
        { 
            GameCache = gameCache;
            PendingGameCache = pendingGameCache;
            GameService = gameService;
            DeckService = deckService;
            CardService = cardService;
        }

        public async Task CreateGame()
        {
            string gameCode = GetUnusedGameCode();
            string hostConnectionId = Context.ConnectionId;

            var pendingGame = new PendingGame(gameCode, hostConnectionId);

            PendingGameCache.AddPendingGame(pendingGame);

            await Clients.Client(hostConnectionId).SendAsync("CreatedPendingGame", gameCode);
        }

        public async Task JoinGame(string gameCode)
        {
            var guestConnectionId = Context.ConnectionId;
            var upperCaseGameCode = gameCode.ToUpper();
            var pendingGame = PendingGameCache.GetPendingGameByGameCode(upperCaseGameCode);

            if (pendingGame is null)
            {
                await Clients.Client(guestConnectionId).SendAsync("InvalidGameCode");
                return;
            }

            var game = GameService.CreateGame(pendingGame.HostConnectionId, guestConnectionId, gameCode);

            GameCache.AddGame(game);
            PendingGameCache.RemovePendingGame(gameCode);

            await UpdatePlayerGameStates(game, "GameStarted");
        }

        public void RearrangeHand(string stringifiedCards)
        {
            var cards = JsonConvert.DeserializeObject<List<Card>>(stringifiedCards);

            if (cards is null)
            {
                return;
            }

            var connectionId = Context.ConnectionId;

            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
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
            }
            else
            {
                game.GuestPlayer.Hand.Cards = cards;
            }
        }

        public async Task MakeMove(string stringifiedMove)
        {
            var move = JsonConvert.DeserializeObject<Move>(stringifiedMove);

            if (move is null)
            {
                return;
            }

            var connectionId = Context.ConnectionId;
            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
            {
                return;
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            var moveWasValid = GameService.MakeMoveIfValid(game, move, playerIsHost);

            if (!moveWasValid)
            {
                // TODO: Penalize client for invalid move.
                return;
            }

            await UpdatePlayerGameStates(game, "GameUpdated");

            if (game.WonBy == PlayerOrNone.None)
            {
                return;
            }

            var winnerConnId = game.WonBy == PlayerOrNone.Host ? game.HostConnectionId : game.GuestConnectionId;
            var loserConnId = game.WonBy == PlayerOrNone.Host ? game.GuestConnectionId : game.HostConnectionId;

            await Clients.Client(winnerConnId).SendAsync("GameOver", "You win.");
            await Clients.Client(loserConnId).SendAsync("GameOver", "You lose.");
        }

        public async Task PassMove()
        {
            var connectionId = Context.ConnectionId;

            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
            {
                return;
            }

            var playerIsHost = connectionId == game.HostConnectionId;
            var hostAndHostTurn = playerIsHost && game.IsHostPlayersTurn;
            var guestAndGuestTurn = !playerIsHost && !game.IsHostPlayersTurn;
            var isPlayersTurn = hostAndHostTurn || guestAndGuestTurn;

            if (!isPlayersTurn)
            {
                return;
            }

            GameService.DrawCardsUntilHandAtFive(game, playerIsHost);

            game.IsHostPlayersTurn = !game.IsHostPlayersTurn;

            await UpdatePlayerGameStates(game, "PassedMove");
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

            var game = GameCache.RemoveGameByConnectionId(connectionId);

            if (game is not null)
            {
                var hostDisconnected = connectionId == game.HostConnectionId;
                var opponentConnectionId = hostDisconnected ? game.GuestConnectionId : game.HostConnectionId;

                await Clients.Client(opponentConnectionId).SendAsync("GameOver", "Opponent Disconnected. You win!");
            }
        }

        private string GetUnusedGameCode()
        {
            var numRetries = 10;
            var currentRetry = 0;

            while (currentRetry < numRetries)
            {
                var gameCode = Guid.NewGuid().ToString()[..4].ToUpper();
                var gameCodeIsUnused = PendingGameCache.GetPendingGameByGameCode(gameCode) is null;

                if (gameCodeIsUnused)
                {
                    return gameCode;
                } else
                {
                    currentRetry++;
                }
            }

            throw new Exception("Unable to generate an unused game code.");
        }

        private async Task UpdatePlayerGameStates(Game game, string messageType)
        {
            await UpdateHostGameState(game, messageType);
            await UpdateGuestGameState(game, messageType);
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
                game.IsHostPlayersTurn,
                game.RedJokerLaneIndex,
                game.BlackJokerLaneIndex
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
                game.IsHostPlayersTurn,
                game.RedJokerLaneIndex,
                game.BlackJokerLaneIndex
                );

            var serializedGuestGameState = JsonConvert.SerializeObject(guestGameState, new StringEnumConverter());
            
            await Clients.Client(game.GuestConnectionId).SendAsync(messageType, serializedGuestGameState);
        }
    }
}