using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameEngineService
    {
        public bool MoveIsValid(Game game, Move move, bool playerIsHost);

        public void MakeMove(Game game, Move move, bool playerIsHost);
    }
}
