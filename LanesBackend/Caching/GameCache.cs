using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Caching
{
    public class GameCache : IGameCache
    {
        private static readonly List<Game> Games = new();

        public void AddGame(Game game)
        {
            Games.Add(game);
        }

        public Game? FindGameByConnectionId(string connectionId)
        {
            var gamesWithConnectionId = Games.Where(game =>
            {
                var hostConnectionIdMatches = game.HostConnectionId == connectionId;
                var guestConnectionIdMatches = game.GuestConnectionId == connectionId;

                return hostConnectionIdMatches || guestConnectionIdMatches;
            });

            var gameWithConnectionId = gamesWithConnectionId.FirstOrDefault();

            return gameWithConnectionId;
        }

        public Game? RemoveGameByConnectionId(string connectionId)
        {
            var game = FindGameByConnectionId(connectionId);

            if (game is not null)
            {
                Games.Remove(game);
            }

            return game;
        }
    }
}
