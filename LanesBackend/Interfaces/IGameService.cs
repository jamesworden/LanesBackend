using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameService
    {
        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode, DurationOption durationOption);

        public bool MakeMove(Game game, Move move, bool playerIsHost);

        public void PassMove(Game game, bool playerIsHost);

        public void RearrangeHand(Game game, bool playerIsHost, List<Card> cards);
    }
}
