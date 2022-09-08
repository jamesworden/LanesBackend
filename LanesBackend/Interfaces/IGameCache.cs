using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameCache
    {
        public void AddGame(Game game);

        public Game? FindGameByConnectionId(string connectionId);

        public Game? RemoveGameByConnectionId(string connectionId);
    }
}
