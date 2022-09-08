using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameService
    {
        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode);

        public bool MakeMoveIfValid(Game game, Move move, string playerConnectionId);
    }
}
