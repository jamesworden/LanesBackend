using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameService
    {
        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode, DurationOption durationOption);

        public Game MakeMove(string connectionId, Move move);

        public void PassMove(Game game, bool playerIsHost);

        public Hand RearrangeHand(string connectionId, List<Card> cards);

        public Game? RemoveGame(string connectionId);
    }
}
