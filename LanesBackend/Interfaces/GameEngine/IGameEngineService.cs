using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameEngineService
    {
        public bool MoveIsValid(Game game, Move move, bool playerIsHost);

        public bool MakeMove(Game game, Move move, bool playerIsHost);
    }
}
