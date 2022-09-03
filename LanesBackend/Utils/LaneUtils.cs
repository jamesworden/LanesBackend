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

        private static void SwitchLanePov(Lane lane)
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
        }

        private static void SwitchHostAndGuestPlayedBy(Card card)
        {
            card.PlayedBy = card.PlayedBy == PlayedBy.Host ? PlayedBy.Guest : PlayedBy.Host;
        }

        private static void ConvertMoveToHostPov(Move move)
        {
            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                placeCardAttempt.TargetLaneIndex = 4 - placeCardAttempt.TargetLaneIndex;
                placeCardAttempt.TargetRowIndex = 6 - placeCardAttempt.TargetRowIndex;
            }
        }

        private static void ConvertMoveToGuestPov(Move move)
        {
            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                placeCardAttempt.TargetLaneIndex = Math.Abs(placeCardAttempt.TargetLaneIndex - 4);
                placeCardAttempt.TargetRowIndex =  Math.Abs(placeCardAttempt.TargetRowIndex - 6);
            }
        }
    }
}
