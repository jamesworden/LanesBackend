using LanesBackend.Models;
using Newtonsoft.Json;

namespace LanesBackend.Utils
{
    public static class MoveChecker
    {
        public static bool IsMoveValid(Move move, Lane lane, bool playerIsHost)
        {
            var clonedMove = JsonConvert.DeserializeObject<Move>(JsonConvert.SerializeObject(move));
            var clonedLane = JsonConvert.DeserializeObject<Lane>(JsonConvert.SerializeObject(lane));

            if (clonedMove == null || clonedLane == null)
            {
                throw new Exception("Error cloning move and lane in IsMoveValid()");
            }

            if (!playerIsHost)
            {
                ConvertMoveToHostPov(clonedMove);
                ConvertLaneToHostPov(clonedLane);
            }

            return IsMoveValidFromHostPov(clonedMove, clonedLane);
        }

        private static void ConvertMoveToHostPov(Move move)
        {
            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                placeCardAttempt.TargetLaneIndex = 4 - placeCardAttempt.TargetLaneIndex;
                placeCardAttempt.TargetRowIndex = 6 - placeCardAttempt.TargetRowIndex;
            }
        }

        private static void ConvertLaneToHostPov(Lane lane)
        {
            lane.Rows.Reverse();

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

        private static bool IsMoveValidFromHostPov(Move move, Lane lane)
        {
            Console.WriteLine(JsonConvert.SerializeObject(lane));
            Console.WriteLine(JsonConvert.SerializeObject(move));
            return true;
        }
    }
}
