using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IMoveChecksService
    {
        public bool IsPlayersTurn(Game game, bool playerIsHost);

        public bool IsEntireMoveOnSameLane(Move move);

        public bool IsAnyPlaceCardAttemptInMiddle(Move move);
    }
}
