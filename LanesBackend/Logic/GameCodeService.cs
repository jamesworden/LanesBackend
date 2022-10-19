using LanesBackend.Interfaces;

namespace LanesBackend.Logic
{
    public class GameCodeService : IGameCodeService
    {
        private readonly IPendingGameCache PendingGameCache;

        public GameCodeService(IPendingGameCache pendingGameCache)
        {
            PendingGameCache = pendingGameCache;
        }

        public string GenerateUniqueGameCode()
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
                }
                else
                {
                    currentRetry++;
                }
            }

            throw new Exception("Unable to generate an unique game code.");
        }
    }
}
