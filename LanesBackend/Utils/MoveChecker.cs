using LanesBackend.Models;
using Newtonsoft.Json;

namespace LanesBackend.Utils
{
    public static class MoveChecker
    {
        public static bool IsMoveValid(Move move, Lane lane, bool playerIsHost)
        {
            var isMoveValid = false;

            LaneUtils.ModifyLaneFromHostPov(lane, playerIsHost, (hostPovLane) =>
            {
                LaneUtils.ModifyMoveFromHostPov(move, playerIsHost, (hostPovMove) =>
                {
                    isMoveValid = IsMoveValidFromHostPov(hostPovMove, hostPovLane);
                });
            });

            return isMoveValid;
        }

        private static bool IsMoveValidFromHostPov(Move move, Lane lane)
        {
            var lastCardPlayed = lane.LastCardPlayed;
            var laneAdvantage = lane.LaneAdvantage;
            var placeCardAttempt = move.PlaceCardAttempts[0]; // For now, assume all moves are one place card attempt.
            var targetRowIndex = placeCardAttempt.TargetRowIndex;
            var card = placeCardAttempt.Card;
            
            var targetCard = GetTopCardOfTargetRow(lane, targetRowIndex);
            var playerPlayedTargetCard = targetCard?.PlayedBy == PlayedBy.Host;

            var moveIsPlayerSide = targetRowIndex < 3;
            var moveIsMiddle = targetRowIndex == 3;
            var moveIsOpponentSide = targetRowIndex > 3;

            var playerHasAdvantage = laneAdvantage == LaneAdvantage.Host;
            var opponentHasAdvantage = laneAdvantage == LaneAdvantage.Guest;
            var noAdvantage = laneAdvantage == LaneAdvantage.None;

            if (moveIsMiddle)
            {
                Console.WriteLine("Client broke the rules: Tried to move in middle row.");
                return false;
            }

            if (moveIsPlayerSide && playerHasAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their own side when they have the advantage.");
                return true;
            }

            if (moveIsOpponentSide && opponentHasAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their opponent's side when they have their opponent has the advantage.");
                return false;
            }

            if (moveIsOpponentSide && noAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their opponent's side when there is no advantage.");
                return false;
            }

            if (moveIsPlayerSide && !AllPreviousRowsOccupied(lane, targetRowIndex))
            {
                Console.WriteLine("Client broke the rules: Tried to move on position where previous rows aren't occupied.");
                return false;
            }

            if (lastCardPlayed is not null && !CardsHaveMatchingSuitOrKind(card, lastCardPlayed))
            {
                Console.WriteLine("Client broke the rules: Tried to play a card that has other suit or other kind than the last card played.");
                return false;
            }

            // Can't reinforce with different suit card.
            if (
              targetCard is not null &&
              playerPlayedTargetCard &&
              !CardsHaveMatchingSuit(targetCard, card)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a different suit.");
                return false;
            }

            // Can't reinforce with a lesser card.
            if (
              targetCard is not null &&
              playerPlayedTargetCard &&
              !CardTrumpsCard(card, targetCard)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a lesser card.");
                return false;
            }

            return true;
        }

        private static Card? GetTopCardOfTargetRow(Lane lane, int targetRowIndex)
        {
            var targetRow = lane.Rows[targetRowIndex];
            var targetRowHasCards = targetRow.Any();

            if (targetRowHasCards)
            {
                var topCard = targetRow.First();
                return topCard;
            }

            return null;
        }

        private static bool AllPreviousRowsOccupied(Lane lane, int targetRowIndex)
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

        private static bool CardsHaveMatchingSuitOrKind(Card card1, Card card2)
        {
            var suitsMatch = card1.Suit == card2.Suit;
            var kindsMatch = card1.Kind == card2.Kind;

            return suitsMatch || kindsMatch;
        }

        private static bool CardsHaveMatchingSuit(Card card1, Card card2)
        {
            return card1.Suit == card2.Suit;
        }

        private static bool CardTrumpsCard(Card attackingCard, Card defendingCard)
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
