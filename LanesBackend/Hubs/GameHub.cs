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

            await UpdatePlayerGameStates(game, "GameUpdated");

            if (game.WonBy == PlayerOrNone.None)
            {
                return;
            }

            var winnerConnId = game.WonBy == PlayerOrNone.Host ? game.HostConnectionId : game.GuestConnectionId;
            var loserConnId = game.WonBy == PlayerOrNone.Host ? game.GuestConnectionId : game.HostConnectionId;

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

            await UpdatePlayerGameStates(game, "PassedMove");
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

            await Clients.Client(game.HostConnectionId).SendAsync("GameOver", "It's a draw.");
            await Clients.Client(game.GuestConnectionId).SendAsync("GameOver", "It's a draw.");
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

            await Clients.Client(winnerConnectionId).SendAsync("GameOver", "Opponent resigned.");
            await Clients.Client(loserConnectionId).SendAsync("GameOver", "Game resigned.");
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
                game.BlackJokerLaneIndex,
                game.GameCreatedTimestampUTC
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
                game.BlackJokerLaneIndex,
                game.GameCreatedTimestampUTC
                );

            var serializedGuestGameState = JsonConvert.SerializeObject(guestGameState, new StringEnumConverter());
            
            await Clients.Client(game.GuestConnectionId).SendAsync(messageType, serializedGuestGameState);
        }
    }
}