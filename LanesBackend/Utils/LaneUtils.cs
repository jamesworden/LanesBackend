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
    }
}
