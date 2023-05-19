using LanesBackend.Interfaces;
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

        private readonly IGameCodeService GameCodeService;

        public GameHub(
            IGameCache gameCache, 
            IPendingGameCache pendingGameCache, 
            IGameService gameService, 
            IGameCodeService gameCodeService)
        { 
            GameCache = gameCache;
            PendingGameCache = pendingGameCache;
            GameService = gameService;
            GameCodeService = gameCodeService;
        }

        public async Task CreateGame()
        {
            string gameCode = GameCodeService.GenerateUniqueGameCode();
            string hostConnectionId = Context.ConnectionId;
            var pendingGame = new PendingGame(gameCode, hostConnectionId);

            PendingGameCache.AddPendingGame(pendingGame);

            var pendingGameView = new PendingGameView(pendingGame.GameCode, pendingGame.DurationOption);
            var serializedPendingGameView = JsonConvert.SerializeObject(pendingGameView, new StringEnumConverter());

            await Clients.Client(hostConnectionId).SendAsync("CreatedPendingGame", serializedPendingGameView);
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

            var game = GameService.CreateGame(pendingGame.HostConnectionId, guestConnectionId, gameCode, pendingGame.DurationOption);

            GameCache.AddGame(game);
            PendingGameCache.RemovePendingGame(gameCode);

            await UpdatePlayerGameViews(game, "GameStarted");
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

            GameService.RearrangeHand(game, playerIsHost, cards);
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

            GameService.MakeMove(game, move, playerIsHost);

            await UpdatePlayerGameViews(game, "GameUpdated");

            if (game.WonBy == PlayerOrNone.None)
            {
                return;
            }

            var winnerConnId = game.WonBy == PlayerOrNone.Host ? game.HostConnectionId : game.GuestConnectionId;
            var loserConnId = game.WonBy == PlayerOrNone.Host ? game.GuestConnectionId : game.HostConnectionId;

            game.GameEndedTimestampUTC = DateTime.UtcNow;

            await Clients.Client(winnerConnId).SendAsync("GameOver", "You win!");
            await Clients.Client(loserConnId).SendAsync("GameOver", "You lose!");

            GameCache.RemoveGameByConnectionId(connectionId);
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

            await UpdatePlayerGameViews(game, "PassedMove");
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

            await Clients.Client(opponentConnectionId).SendAsync("DrawOffered");
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

            await Clients.Client(game.HostConnectionId).SendAsync("GameOver", "It's a draw.");
            await Clients.Client(game.GuestConnectionId).SendAsync("GameOver", "It's a draw.");

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

            await Clients.Client(winnerConnectionId).SendAsync("GameOver", "Opponent resigned.");
            await Clients.Client(loserConnectionId).SendAsync("GameOver", "Game resigned.");

            GameCache.RemoveGameByConnectionId(connectionId);
        }

        public async Task SelectDurationOption(string stringifiedDurationOption)
        {
            var connectionId = Context.ConnectionId;
            var pendingGame = PendingGameCache.GetPendingGameByConnectionId(connectionId);

            if (pendingGame is null)
            {
                return;
            }

            var durationOption = Enum.Parse<DurationOption>(stringifiedDurationOption);
            pendingGame.DurationOption = durationOption;
            PendingGameCache.RemovePendingGame(pendingGame.GameCode);
            PendingGameCache.AddPendingGame(pendingGame);
            var pendingGameView = new PendingGameView(pendingGame.GameCode, pendingGame.DurationOption);
            var serializedPendingGameView = JsonConvert.SerializeObject(pendingGameView, new StringEnumConverter());

            await Clients.Client(connectionId).SendAsync("PendingGameUpdated", serializedPendingGameView);
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

            await Clients.Client(game.HostConnectionId).SendAsync("GameOver", "Your timer ran out. You lose.");
            await Clients.Client(game.GuestConnectionId).SendAsync("GameOver", "Your opponent's timer ran out. You win!");

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

            await Clients.Client(game.GuestConnectionId).SendAsync("GameOver", "Your timer ran out. You lose.");
            await Clients.Client(game.HostConnectionId).SendAsync("GameOver", "Your opponent's timer ran out. You win!");

            GameCache.RemoveGameByConnectionId(connectionId);
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
                game.GameEndedTimestampUTC = DateTime.UtcNow;

                await Clients.Client(opponentConnectionId).SendAsync("GameOver", "Opponent Disconnected. You win!");
            }
        }

        private async Task UpdatePlayerGameViews(Game game, string messageType)
        {
            await UpdateHostGameView(game, messageType);
            await UpdateGuestGameView(game, messageType);
        }

        private async Task UpdateHostGameView(Game game, string messageType)
        {
            var hostGameView = new PlayerGameView(
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand.Cards.Count,
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand,
                game.Lanes,
                true,
                game.IsHostPlayersTurn,
                game.RedJokerLaneIndex,
                game.BlackJokerLaneIndex,
                game.GameCreatedTimestampUTC,
                game.MovesMade,
                game.DurationOption,
                game.GameEndedTimestampUTC
                );

            var serializedHostGameState = JsonConvert.SerializeObject(hostGameView, new StringEnumConverter());

            await Clients.Client(game.HostConnectionId).SendAsync(messageType, serializedHostGameState);
        }

        private async Task UpdateGuestGameView(Game game, string messageType)
        {
            var guestGameView = new PlayerGameView(
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand.Cards.Count,
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand,
                game.Lanes,
                false,
                game.IsHostPlayersTurn,
                game.RedJokerLaneIndex,
                game.BlackJokerLaneIndex,
                game.GameCreatedTimestampUTC,
                game.MovesMade,
                game.DurationOption,
                game.GameEndedTimestampUTC
                );

            var serializedGuestGameState = JsonConvert.SerializeObject(guestGameView, new StringEnumConverter());
            
            await Clients.Client(game.GuestConnectionId).SendAsync(messageType, serializedGuestGameState);
        }
    }
}