using LanesBackend.Models;

namespace LanesBackend.Utils
{
    public static class MoveChecker
    {
        public static bool IsMoveValidFromHostPov(Move move, Lane lane)
        {
            var lastCardPlayed = lane.LastCardPlayed;
            var laneAdvantage = lane.LaneAdvantage;
            var placeCardAttempt = move.PlaceCardAttempts[0]; // For now, assume all moves are one place card attempt.
            var targetRowIndex = placeCardAttempt.TargetRowIndex;
            var card = placeCardAttempt.Card;
            
            var targetCard = LaneUtils.GetTopCardOfTargetRow(lane, targetRowIndex);
            var playerPlayedTargetCard = targetCard?.PlayedBy == PlayerOrNone.Host;

            var moveIsPlayerSide = targetRowIndex < 3;
            var moveIsMiddle = targetRowIndex == 3;
            var moveIsOpponentSide = targetRowIndex > 3;

            var playerHasAdvantage = laneAdvantage == PlayerOrNone.Host;
            var opponentHasAdvantage = laneAdvantage == PlayerOrNone.Guest;
            var noAdvantage = laneAdvantage == PlayerOrNone.None;

            if (lane.WonBy != PlayerOrNone.None)
            {
                Console.WriteLine("Client broke the rules: Tried to move a lane that has been won.");
                return false;
            }

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

            var playedAceToNukeRow = card.Kind == Kind.Ace && GuestAceTopOfAnyRow(lane);

            if (lastCardPlayed is not null && !LaneUtils.CardsHaveMatchingSuitOrKind(card, lastCardPlayed) && !playedAceToNukeRow)
            {
                Console.WriteLine("Client broke the rules: Tried to play a card that has other suit or other kind than the last card played OR not an ace to nuke the row.");
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

            // Can't reinforce a lesser card.
            if (
              targetCard is not null &&
              playerPlayedTargetCard &&
              !LaneUtils.CardTrumpsCard(card, targetCard)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a lesser card.");
                return false;
            }

            // Can't capture a lesser card.
            if (
              targetCard is not null &&
              card.Suit == targetCard.Suit &&
              !LaneUtils.CardTrumpsCard(card, targetCard)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a lesser card.");
                return false;
            }

            return true;
        }

        private static bool GuestAceTopOfAnyRow(Lane lane)
        {
            foreach(var row in lane.Rows)
            {
                var topCardIndex = row.Count() - 1;
                var topCard = row[topCardIndex];

                if (topCard is null)
                {
                    continue;
                }

                var topCardIsAce = topCard.Kind == Kind.Ace;
                var topCardPlayedbyGuest = topCard.PlayedBy == PlayerOrNone.Guest;

                if (topCardIsAce && topCardPlayedbyGuest)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
