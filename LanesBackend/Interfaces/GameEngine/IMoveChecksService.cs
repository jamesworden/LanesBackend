using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IMoveChecksService
    {
        public bool IsPlayersTurn(Game game, bool playerIsHost);

        public bool IsEntireMoveOnSameLane(Move move);

        public bool IsAnyPlaceCardAttemptInMiddle(Move move);

        public bool AllPreviousRowsOccupied(Lane lane, int targetRowIndex);

        public bool AllFollowingRowsOccupied(Lane lane, int targetRowIndex);

        public bool OpponentAceOnTopOfAnyRow(Lane algoLane, bool playerIsHost);

        public bool CardTrumpsCard(Card attackingCard, Card defendingCard);
    }
}
