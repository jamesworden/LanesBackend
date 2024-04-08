﻿using LanesBackend.Interfaces;
using LanesBackend.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using LanesBackend.Exceptions;

namespace LanesBackend.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameCache GameCache;

        private readonly IGameService GameService;

        private readonly IPendingGameService PendingGameService;

        private readonly IGameBroadcaster GameBroadcaster;

        public GameHub(
            IGameCache gameCache, 
            IGameService gameService, 
            IGameBroadcaster gameBroadcaster,
            IPendingGameService pendingGameService)
        { 
            GameCache = gameCache;
            GameService = gameService;
            PendingGameService = pendingGameService;
            GameBroadcaster = gameBroadcaster;
        }

        public async Task CreateGame()
        {
            string hostConnectionId = Context.ConnectionId;

            try
            {
                var pendingGame = PendingGameService.CreatePendingGame(hostConnectionId);
                var pendingGameView = new PendingGameView(pendingGame.GameCode, pendingGame.DurationOption);
                var serializedPendingGameView = JsonConvert.SerializeObject(pendingGameView, new StringEnumConverter());
                await Clients.Client(hostConnectionId).SendAsync(MessageType.CreatedPendingGame, serializedPendingGameView);
            } catch (Exception ex)
            { 
            }
        }

        public async Task JoinGame(string gameCode)
        {
            var guestConnectionId = Context.ConnectionId;

            try
            {
                var game = PendingGameService.JoinPendingGame(gameCode, guestConnectionId);
                await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameStarted);
            } catch (PendingGameNotExistsException)
            {
                await Clients.Client(guestConnectionId).SendAsync(MessageType.InvalidGameCode);
            }
            catch (Exception ex)
            {
            }
        }

        public void RearrangeHand(string stringifiedCards)
        {
            var connectionId = Context.ConnectionId;
            var cards = JsonConvert.DeserializeObject<List<Card>>(stringifiedCards);

            if (cards is null)
            {
                return;
            }

            try
            {
                GameService.RearrangeHand(connectionId, cards);
            } 
            catch (ContainsDifferentCardsException)
            { 
            }
            catch (GameNotExistsException)
            {
            } catch (Exception ex)
            {

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

            try
            {
                var game = GameService.MakeMove(connectionId, move);
                await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.GameUpdated);

                if (game.WonBy == PlayerOrNone.None)
                {
                    return;
                }

                var winnerConnId = game.WonBy == PlayerOrNone.Host ? game.HostConnectionId : game.GuestConnectionId;
                var loserConnId = game.WonBy == PlayerOrNone.Host ? game.GuestConnectionId : game.HostConnectionId;

                await Clients.Client(winnerConnId).SendAsync(MessageType.GameOver, "You win!");
                await Clients.Client(loserConnId).SendAsync(MessageType.GameOver, "You lose!");
            }
            catch (GameNotExistsException)
            {
            }
            catch
            {
            }
        }

        public async Task PassMove()
        {
            var connectionId = Context.ConnectionId;

            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
            {
                return;
            }

            var playerIsHost = game.HostConnectionId == connectionId;

            GameService.PassMove(game, playerIsHost);

            await GameBroadcaster.BroadcastPlayerGameViews(game, MessageType.PassedMove);
        }

        public async Task OfferDraw()
        {
            var connectionId = Context.ConnectionId;
            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
            {
                return;
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            var opponentConnectionId = playerIsHost ? game.GuestConnectionId : game.HostConnectionId;

            await Clients.Client(opponentConnectionId).SendAsync(MessageType.DrawOffered);
        }

        public async Task AcceptDrawOffer()
        {
            var connectionId = Context.ConnectionId;
            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
            {
                return;
            }

            game.GameEndedTimestampUTC = DateTime.UtcNow;

            await Clients.Client(game.HostConnectionId).SendAsync(MessageType.GameOver, "It's a draw.");
            await Clients.Client(game.GuestConnectionId).SendAsync(MessageType.GameOver, "It's a draw.");

            GameCache.RemoveGameByConnectionId(connectionId);
        }

        public async Task ResignGame()
        {
            var connectionId = Context.ConnectionId;
            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
            {
                return;
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            var winnerConnectionId = playerIsHost ? game.GuestConnectionId : game.HostConnectionId;
            var loserConnectionId = playerIsHost ? game.HostConnectionId : game.GuestConnectionId;

            game.GameEndedTimestampUTC = DateTime.UtcNow;

            await Clients.Client(winnerConnectionId).SendAsync(MessageType.GameOver, "Opponent resigned.");
            await Clients.Client(loserConnectionId).SendAsync(MessageType.GameOver, "Game resigned.");

            GameCache.RemoveGameByConnectionId(connectionId);
        }

        public async Task SelectDurationOption(string stringifiedDurationOption)
        {
            var connectionId = Context.ConnectionId;
            var durationOption = Enum.Parse<DurationOption>(stringifiedDurationOption);

            try
            {
                var pendingGame = PendingGameService.SelectDurationOption(connectionId, durationOption);
                var pendingGameView = new PendingGameView(pendingGame.GameCode, pendingGame.DurationOption);
                var serializedPendingGameView = JsonConvert.SerializeObject(pendingGameView, new StringEnumConverter());
                await Clients.Client(connectionId).SendAsync(MessageType.PendingGameUpdated, serializedPendingGameView);
            }
            catch (PendingGameNotExistsException)
            {
            }
            catch (Exception)
            {
            }
        }

        public async Task CheckHostForEmptyTimer()
        {
            var connectionId = Context.ConnectionId;
            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
            {
                return;
            }

            // TODO [Security Hardening]: Actually check if host has an empty timer.

            game.GameEndedTimestampUTC = DateTime.UtcNow;

            await Clients.Client(game.HostConnectionId).SendAsync(MessageType.GameOver, "Your timer ran out. You lose.");
            await Clients.Client(game.GuestConnectionId).SendAsync(MessageType.GameOver, "Your opponent's timer ran out. You win!");

            GameCache.RemoveGameByConnectionId(connectionId);
        }

        public async Task CheckGuestForEmptyTimer()
        {
            var connectionId = Context.ConnectionId;
            var game = GameCache.FindGameByConnectionId(connectionId);

            if (game is null)
            {
                return;
            }

            // TODO [Security Hardening]: Actually check if guest has an empty timer.

            game.GameEndedTimestampUTC = DateTime.UtcNow;

            await Clients.Client(game.GuestConnectionId).SendAsync(MessageType.GameOver, "Your timer ran out. You lose.");
            await Clients.Client(game.HostConnectionId).SendAsync(MessageType.GameOver, "Your opponent's timer ran out. You win!");

            GameCache.RemoveGameByConnectionId(connectionId);
        }

        public async override Task OnDisconnectedAsync(Exception? _)
        {
            var connectionId = Context.ConnectionId;

            var pendingGame = PendingGameService.RemovePendingGame(connectionId);
            if (pendingGame is not null)
            {
                return;
            }

            var game = GameService.RemoveGame(connectionId);
            if (game is not null)
            {
                var hostDisconnected = connectionId == game.HostConnectionId;
                var opponentConnectionId = hostDisconnected ? game.GuestConnectionId : game.HostConnectionId;
                await Clients.Client(opponentConnectionId).SendAsync(MessageType.GameOver, "Opponent Disconnected. You win!");
            }
        }
    }
}