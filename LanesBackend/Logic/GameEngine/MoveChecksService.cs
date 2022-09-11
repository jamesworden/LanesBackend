using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class MoveChecksService : IMoveChecksService
    {
        public bool IsPlayersTurn(Game game, bool playerIsHost)
        {
            var hostAndHostTurn = playerIsHost && game.IsHostPlayersTurn;
            var guestAndGuestTurn = !playerIsHost && !game.IsHostPlayersTurn;
            var isPlayersTurn = hostAndHostTurn || guestAndGuestTurn;

            return isPlayersTurn;
        }

        public bool IsEntireMoveOnSameLane(Move move)
        {
            var noPlaceCardAttempts = move.PlaceCardAttempts.Count <= 0;
            if (noPlaceCardAttempts)
            {
                return true;
            }

            var firstPlaceCardAttempt = move.PlaceCardAttempts[0];
            var lastTargetLaneIndex = firstPlaceCardAttempt.TargetLaneIndex;

            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                if (placeCardAttempt.TargetLaneIndex != lastTargetLaneIndex)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsAnyPlaceCardAttemptInMiddle(Move move)
        {
            foreach(var placeCardAttempt in move.PlaceCardAttempts)
            {
                if (placeCardAttempt.TargetRowIndex == 3)
                {
                    return true;
                }
            }

            return false;
        }

        public bool AllPreviousRowsOccupied(Lane lane, int targetRowIndex)
        {
            for (int i = 0; i < targetRowIndex; i++)
            {
                var previousLane = lane.Rows[i];
                var previousLaneNotOccupied = previousLane.Count == 0;

                if (previousLaneNotOccupied)
                {
                    return false;
                }
            }

            return true;
        }

        public bool AllFollowingRowsOccupied(Lane lane, int targetRowIndex)
        {
            for (int i = lane.Rows.Length - 1; i > targetRowIndex; i--)
            {
                var followingLane = lane.Rows[i];
                var followingLaneNotOccupied = followingLane.Count == 0;

                if (followingLaneNotOccupied)
                {
                    return false;
                }
            }

            return true;
        }

        public bool OpponentAceOnTopOfAnyRow(Lane algoLane, bool playerIsHost)
        {
            foreach (var row in algoLane.Rows)
            {
                if (row.Count <= 0)
                {
                    continue;
                }

                var topCard = row.Last();

                var topCardIsAce = topCard.Kind == Kind.Ace;
                var topCardPlayedByOpponent = playerIsHost ?
                    topCard.PlayedBy == PlayerOrNone.Guest :
                    topCard.PlayedBy == PlayerOrNone.Host;
                if (topCardIsAce && topCardPlayedByOpponent)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CardTrumpsCard(Card attackingCard, Card defendingCard)
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
