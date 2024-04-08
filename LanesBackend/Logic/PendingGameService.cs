using LanesBackend.Exceptions;
using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class PendingGameService : IPendingGameService
    {
        private readonly IGameCodeService GameCodeService;

        private readonly IPendingGameCache PendingGameCache;

        private readonly IGameService GameService;

        public PendingGameService(
            IGameCodeService gameCodeService,
            IPendingGameCache pendingGameCache,
            IGameService gameService)
        {
            GameCodeService = gameCodeService;
            PendingGameCache = pendingGameCache;
            GameService = gameService;
        }

        public PendingGame CreatePendingGame(string hostConnectionId)
        {
            string gameCode = GameCodeService.GenerateUniqueGameCode();
            var pendingGame = new PendingGame(gameCode, hostConnectionId);
            PendingGameCache.AddPendingGame(pendingGame);
            return pendingGame;
        }

        public Game JoinPendingGame(string gameCode, string guestConnectionId)
        {
            var upperCaseGameCode = gameCode.ToUpper();
            var pendingGame = PendingGameCache.GetPendingGameByGameCode(upperCaseGameCode);
            if (pendingGame is null)
            {
                throw new PendingGameNotExistsException();
            }

            var game = GameService.CreateGame(
                pendingGame.HostConnectionId, 
                guestConnectionId, 
                gameCode, 
                pendingGame.DurationOption);

            PendingGameCache.RemovePendingGame(gameCode);

            return game;
        }

        public PendingGame SelectDurationOption(string connectionId, DurationOption durationOption)
        {
            var pendingGame = PendingGameCache.GetPendingGameByConnectionId(connectionId);
            if (pendingGame is null)
            {
                throw new PendingGameNotExistsException();
            }

            pendingGame.DurationOption = durationOption;
            PendingGameCache.RemovePendingGame(pendingGame.GameCode);
            PendingGameCache.AddPendingGame(pendingGame);
            return pendingGame;
        }

        public PendingGame? RemovePendingGame(string connectionId)
        {
            var pendingGame = PendingGameCache.GetPendingGameByConnectionId(connectionId);
            if (pendingGame is not null)
            {
                PendingGameCache.RemovePendingGame(pendingGame.GameCode);
            }
            return pendingGame;
        }
    }
}
