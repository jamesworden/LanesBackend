using LanesBackend.Models;

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
            
            var targetCard = LaneUtils.GetTopCardOfTargetRow(lane, targetRowIndex);
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

            if (moveIsPlayerSide && !LaneUtils.AllPreviousRowsOccupied(lane, targetRowIndex))
            {
                Console.WriteLine("Client broke the rules: Tried to move on position where previous rows aren't occupied.");
                return false;
            }

            if (lastCardPlayed is not null && !LaneUtils.CardsHaveMatchingSuitOrKind(card, lastCardPlayed))
            {
                Console.WriteLine("Client broke the rules: Tried to play a card that has other suit or other kind than the last card played.");
                return false;
            }

            // Can't reinforce with different suit card.
            if (
              targetCard is not null &&
              playerPlayedTargetCard &&
              !LaneUtils.CardsHaveMatchingSuit(targetCard, card)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a different suit.");
                return false;
            }

            // Can't reinforce with a lesser card.
            if (
              targetCard is not null &&
              playerPlayedTargetCard &&
              !LaneUtils.CardTrumpsCard(card, targetCard)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a lesser card.");
                return false;
            }

            return true;
        }
    }
}
