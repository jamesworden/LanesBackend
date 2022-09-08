using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Caching
{
    public class PendingGameCache : IPendingGameCache
    {
        private static readonly Dictionary<string, string> PendingGameCodeToHostConnectionId = new();

        public void AddPendingGame(PendingGame pendingGame)
        {
            PendingGameCodeToHostConnectionId.Add(pendingGame.GameCode, pendingGame.HostConnectionId);
        }

        public PendingGame? GetPendingGameByGameCode(string gameCode)
        {
            PendingGameCodeToHostConnectionId.TryGetValue(gameCode, out var hostConnectionId);

            if (hostConnectionId is null)
            {
                return null;
            }

            var pendingGame = new PendingGame
            {
                GameCode = gameCode,
                HostConnectionId = hostConnectionId
            };

            return pendingGame;
        }

        public PendingGame? GetPendingGameByConnectionId(string hostConnectionId)
        {
            var pendingGameCode = PendingGameCodeToHostConnectionId.FirstOrDefault(row => row.Value == hostConnectionId).Key;

            if (pendingGameCode is null)
            {
                return null;
            }

            var pendingGame = new PendingGame
            {
                GameCode = pendingGameCode,
                HostConnectionId = hostConnectionId
            };

            return pendingGame;
        }

        public bool RemovePendingGame(string gameCode)
        {
            var pendingGameRemoved = PendingGameCodeToHostConnectionId.Remove(gameCode);

            return pendingGameRemoved;
        }
    }
}
