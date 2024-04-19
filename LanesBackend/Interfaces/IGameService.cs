using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IGameService
    {
        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode, DurationOption durationOption, bool playerIsHost);

        public (Game, IEnumerable<MoveMadeResult>) MakeMove(string connectionId, Move move, List<Card>? rearrangedCardsInHand);

        public Game PassMove(string connectionId);

        public Hand RearrangeHand(string connectionId, List<Card> cards);

        public Game? RemoveGame(string connectionId);

        public Game? FindGame(string connectionId);

        public Game AcceptDrawOffer(string connectionId);

        public Game ResignGame(string connectionId);

        public Game EndGame(string connectionId);

        public List<CandidateMove> GetCandidateMoves(Game game, bool forHostPlayer);

        public Game UpdateGame(TestingGameData testingGameData, string gameCode);
    }
}
