using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameService
    {
        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode);

        public bool MakeMoveIfValid(Game game, Move move, bool playerIsHost);

        public int RemoveCardsFromHand(Game game, bool playerIsHost, Move move);

        public void DrawCardsFromDeck(Game game, bool playerIsHost, int numCardsToDraw);

        public void DrawCardsUntilHandAtFive(Game game, bool playerIsHost);
    }
}
