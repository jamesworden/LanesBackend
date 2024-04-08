using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameService
    {
        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode, DurationOption durationOption);

        public Game MakeMove(string connectionId, Move move);

        public Game PassMove(string connectionId);

        public Hand RearrangeHand(string connectionId, List<Card> cards);

        public Game? RemoveGame(string connectionId);

        public Game? FindGame(string connectionId);

        public Game AcceptDrawOffer(string connectionId);

        public Game ResignGame(string connectionId);
    }
}
