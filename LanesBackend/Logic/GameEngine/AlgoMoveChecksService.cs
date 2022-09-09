using LanesBackend.Interfaces.GameEngine;
using LanesBackend.Models;
using LanesBackend.Models.GameEngine;

namespace LanesBackend.Logic.GameEngine
{
    public class AlgoMoveChecksService : IAlgoMoveChecksService
    {
        public bool AllPreviousRowsOccupied(AlgoLane algoLane, int targetRowIndex)
        {
            for (int i = 0; i < targetRowIndex; i++)
            {
                var previousLane = algoLane.Rows[i];
                var previousLaneNotOccupied = previousLane.Count == 0;

                if (previousLaneNotOccupied)
                {
                    return false;
                }
            }

            return true;
        }

        public bool OpponentAceOnTopOfAnyRow(AlgoLane algoLane)
        {
            foreach(var row in algoLane.Rows)
            {
                if (row.Count <= 0)
                {
                    continue;
                }

                var topCard = row.Last();

                if (topCard is not null && topCard.Kind == Kind.Ace && topCard.PlayedBy == AlgoPlayer.Opponent)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CardTrumpsCard(AlgoCard attackingCard, AlgoCard defendingCard)
        {
            var hasSameSuit = attackingCard.Suit == defendingCard.Suit;
            var hasSameKind = attackingCard.Kind == defendingCard.Kind;

            if (!hasSameSuit)
            {
                return hasSameKind;
            }

            var attackingKindValue = (int)attackingCard.Kind;
            var defendingKindValue = (int)defendingCard.Kind;

            return attackingKindValue > defendingKindValue;
        }
    }
}
