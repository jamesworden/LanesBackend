using LanesBackend.Models.GameEngine;

namespace LanesBackend.Interfaces.GameEngine
{
    public interface IAlgoMoveChecksService
    {
        public bool AllPreviousRowsOccupied(AlgoLane algoLane, int targetRowIndex);

        public bool OpponentAceOnTopOfAnyRow(AlgoLane algoLane);

        public bool CardTrumpsCard(AlgoCard attackingCard, AlgoCard defendingCard);
    }
}
