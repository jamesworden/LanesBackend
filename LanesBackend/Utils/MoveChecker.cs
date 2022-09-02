using LanesBackend.Models;
using Newtonsoft.Json;

namespace LanesBackend.Utils
{
    public static class MoveChecker
    {
        public static bool IsMoveValid(Move move, Lane lane, bool playerIsHost)
        {
            var clonedMove = CloneMove(move);
            var clonedLane = CloneLane(lane);

            if (!playerIsHost)
            {
                ConvertMoveToHostPov(clonedMove);
                ConvertLaneToHostPov(clonedLane);
            }

            return IsMoveValidFromHostPov(clonedMove, clonedLane);
        }

        private static Lane CloneLane(Lane lane)
        {
            var serializedLane = JsonConvert.SerializeObject(lane);
            var clonedLane = JsonConvert.DeserializeObject<Lane>(serializedLane);
            if (clonedLane == null) throw new Exception("Error cloning lane in CloneLane()");
            // Serialize rows seperately because it doesn't work when serializing and deserializing the lane itself.
            var serializedRows = JsonConvert.SerializeObject(lane.Rows);
            var clonedRows = JsonConvert.DeserializeObject<List<Card>[]>(serializedRows);
            if (clonedRows == null) throw new Exception("Error cloning rows in CloneLane()");
            clonedLane.Rows = clonedRows;
            return clonedLane;
        }

        private static Move CloneMove(Move move)
        {
            var serializedMove = JsonConvert.SerializeObject(move);
            var clonedMove = JsonConvert.DeserializeObject<Move>(serializedMove);
            if (clonedMove == null) throw new Exception("Error cloning move in CloneMove()");
            return clonedMove;
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
            var lastCardPlayed = lane.LastCardPlayed;
            var laneAdvantage = lane.LaneAdvantage;
            var placeCardAttempt = move.PlaceCardAttempts[0]; // For now, assume all moves are one place card attempt.
            var targetRowIndex = placeCardAttempt.TargetRowIndex;
            var targetLaneIndex = placeCardAttempt.TargetLaneIndex;
            var card = placeCardAttempt.Card;
            
            var targetCard = GetTopCardOfTargetRow(lane, targetLaneIndex);
            var playerPlayedTargetCard = targetCard?.PlayedBy == PlayedBy.Host;

            var moveIsPlayerSide = targetRowIndex < 3;
            var moveIsMiddle = targetRowIndex == 3;
            var moveIsOpponentSide = targetRowIndex > 3;

            var playerHasAdvantage = laneAdvantage == LaneAdvantage.Host;
            var opponentHasAdvantage = laneAdvantage == LaneAdvantage.Guest;
            var noAdvantage = laneAdvantage == LaneAdvantage.None;

            if (moveIsMiddle)
            {
                return false;
            }

            if (moveIsPlayerSide && playerHasAdvantage)
            {
                return true;
            }

            if (moveIsOpponentSide && opponentHasAdvantage)
            {
                return false;
            }

            if (moveIsOpponentSide && noAdvantage)
            {
                return false;
            }

            if (moveIsPlayerSide && !AllPreviousRowsOccupied(lane, targetRowIndex))
            {
                return false;
            }

            if (lastCardPlayed is not null && !CardsHaveMatchingSuitOrKind(card, lastCardPlayed))
            {
                return false;
            }

            //// Can't reinforce with different suit card.
            //if (
            //  targetCard &&
            //  playerPlayedTargetCard &&
            //  !CardsHaveMatchingSuit(targetCard, Card)
            //)
            //{
            //    return false;
            //}

            //// Can't reinforce with a lesser card.
            //if (
            //  targetCard &&
            //  playerPlayedTargetCard &&
            //  !CardTrumpsCard(Card, targetCard)
            //)
            //{
            //    return false;
            //}

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
    }
}
