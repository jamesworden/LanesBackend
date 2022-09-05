using LanesBackend.Models;

namespace LanesBackend.Utils
{
    public static class LaneUtils
    {
        public static void ModifyLaneFromHostPov(Lane lane, bool playerIsHost, Action<Lane> hostPovLane)
        {
            var playerIsGuest = !playerIsHost;

            if (playerIsGuest)
            {
                SwitchLanePov(lane);
            }

            hostPovLane(lane);

            if (playerIsGuest)
            {
                SwitchLanePov(lane);
            }
        }

        public static void ModifyMoveFromHostPov(Move move, bool playerIsHost, Action<Move> hostPovMove)
        {
            var playerIsGuest = !playerIsHost;

            if (playerIsGuest)
            {
                ConvertMoveToHostPov(move);
            }

            hostPovMove(move);

            if (playerIsGuest)
            {
                ConvertMoveToGuestPov(move);
            }
        }

        public static void ModifyPlaceCardAttemptFromHostPov(PlaceCardAttempt placeCardAttempt, bool playerIsHost, Action<PlaceCardAttempt> placeCardAttemptHostPov)
        {
            var playerIsGuest = !playerIsHost;

            if (playerIsGuest)
            {
                ConvertPlaceCardAttemptToHostPov(placeCardAttempt);
            }

            placeCardAttemptHostPov(placeCardAttempt);

            if (playerIsGuest)
            {
                ConvertPlaceCardAttemptToGuestPov(placeCardAttempt);
            }
        }

        public static void SwitchLanePov(Lane lane)
        {
            lane.Rows = lane.Rows.Reverse().ToArray();

            foreach (var row in lane.Rows)
            {
                foreach (var card in row)
                {
                    SwitchHostAndGuestPlayedBy(card);
                }
            }

            if (lane.LastCardPlayed != null)
            {
                SwitchHostAndGuestPlayedBy(lane.LastCardPlayed);
            }

            SwitchLaneAdvantage(lane);
        }

        public static void SwitchHostAndGuestPlayedBy(Card card)
        {
            if (card.PlayedBy != PlayerOrNone.None)
            {
                card.PlayedBy = card.PlayedBy == PlayerOrNone.Host ? PlayerOrNone.Guest : PlayerOrNone.Host;
            }
        }

        public static void ConvertMoveToHostPov(Move move)
        {
            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                ConvertPlaceCardAttemptToHostPov(placeCardAttempt);
            }
        }

        public static void ConvertPlaceCardAttemptToHostPov(PlaceCardAttempt placeCardAttempt)
        {
            placeCardAttempt.TargetLaneIndex = 4 - placeCardAttempt.TargetLaneIndex;
            placeCardAttempt.TargetRowIndex = 6 - placeCardAttempt.TargetRowIndex;
        }

        public static void ConvertMoveToGuestPov(Move move)
        {
            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                ConvertPlaceCardAttemptToGuestPov(placeCardAttempt);
            }
        }

        public static void ConvertPlaceCardAttemptToGuestPov(PlaceCardAttempt placeCardAttempt)
        {
            placeCardAttempt.TargetLaneIndex = Math.Abs(placeCardAttempt.TargetLaneIndex - 4);
            placeCardAttempt.TargetRowIndex = Math.Abs(placeCardAttempt.TargetRowIndex - 6);
        }

        public static Card? GetTopCardOfTargetRow(Lane lane, int targetRowIndex)
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

        public static bool AllPreviousRowsOccupied(Lane lane, int targetRowIndex)
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

        public static bool CardsHaveMatchingSuitOrKind(Card card1, Card card2)
        {
            var suitsMatch = card1.Suit == card2.Suit;
            var kindsMatch = card1.Kind == card2.Kind;

            return suitsMatch || kindsMatch;
        }

        public static bool CardsHaveMatchingSuit(Card card1, Card card2)
        {
            return card1.Suit == card2.Suit;
        }

        public static bool CardTrumpsCard(Card attackingCard, Card defendingCard)
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

        public static void SwitchLaneAdvantage(Lane lane)
        {
            if (lane.LaneAdvantage != PlayerOrNone.None)
            {
                lane.LaneAdvantage = lane.LaneAdvantage == PlayerOrNone.Host ? PlayerOrNone.Guest : PlayerOrNone.Host;
            }
        }
    }
}
