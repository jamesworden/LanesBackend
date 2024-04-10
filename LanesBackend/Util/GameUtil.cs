using LanesBackend.Models;

namespace LanesBackend.Util
{
    public static class GameUtil
    {
        public static string? GetReasonIfMoveInvalid(Move move, Game game, bool playerIsHost)
        {
            if (!IsPlayersTurn(game, playerIsHost))
            {
                return "It's not your turn!";
            }
            if (!move.PlaceCardAttempts.Any())
            {
                return "You need to place a card!";
            }
            if (move.PlaceCardAttempts.Count > 4)
            {
                return "You placed too many cards!";
            }
            if (move.PlaceCardAttempts.Any(attempt => attempt.TargetRowIndex == 3))
            {
                return "You can't place a card in the middle!";
            }
            if (move.PlaceCardAttempts.Select(attempt => attempt.TargetLaneIndex).Distinct().Count() > 1)
            {
                return "You can't place cards on different lanes!";
            }
            if (move.PlaceCardAttempts.Select(attempt => attempt.TargetRowIndex).Distinct().Count() < move.PlaceCardAttempts.Count)
            {
                return "You can't place cards on the same position!";
            }
            if (!ContainsConsecutivePlaceCardAttempts(move.PlaceCardAttempts))
            {
                return "You can't place cards that are separate from one another!";
            }
            if (move.PlaceCardAttempts.Select(attempt => attempt.Card.Kind).Distinct().Count() > 1)
            {
                return "Placing multiple cards must be of the same kind!";
            }
            if (TriedToCaptureDistantRow(game, move, playerIsHost))
            {
                return "You can't capture this position yet!";
            }
            if (TargetLaneHasBeenWon(game, move))
            {
                return "This lane was won already!";
            }
            if (TriedToCaptureGreaterCard(game, move, playerIsHost))
            {
                return "You can't capture a greater card!";
            }
            if (StartedMovePlayerSide(game, move, playerIsHost) && PlayerHasAdvantage(game, move, playerIsHost))
            {
                return "You must attack this lane!";
            }
            if (StartedMoveOpponentSide(game, move, playerIsHost) && OpponentHasAdvantage(game, move, playerIsHost))
            {
                return "You must defend this lane!";
            }
            if (StartedMoveOpponentSide(game, move, playerIsHost) && LaneHasNoAdvantage(game, move))
            {
                return "You aren't ready to attack here yet.";
            }
            if (!SuitOrKindMatchesLastCardPlayed(game, move, playerIsHost))
            {
                return "This card can't be placed here.";
            }
            if (TriedToReinforceGreaterCard(game, move, playerIsHost))
            {
                return "You can't reinforce a greater card!";
            }
            return null;
        }

        public static bool IsOffensive(PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            return (placeCardAttempt.TargetLaneIndex > 3 && playerIsHost)
                || (placeCardAttempt.TargetLaneIndex < 3 && !playerIsHost);
        }

        public static bool IsToMiddle(PlaceCardAttempt placeCardAttempt)
        {
            return placeCardAttempt.TargetLaneIndex == 3;
        }

        public static bool IsDefensive(PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            return (placeCardAttempt.TargetLaneIndex < 3 && playerIsHost)
                || (placeCardAttempt.TargetLaneIndex > 3 && !playerIsHost);
        }

        public static bool IsPlayersTurn(Game game, bool playerIsHost)
        {
            return game.IsHostPlayersTurn && playerIsHost 
                || !game.IsHostPlayersTurn && !playerIsHost;
        }

        public static bool SuitMatches(Card card1, Card card2)
        {
            return card1.Suit == card2.Suit;
        }

        public static bool KindMatches(Card card1, Card card2)
        {
            return card1.Kind == card2.Kind;
        }

        public static bool SuitAndKindMatches(Card card1, Card card2)
        {
            return SuitMatches(card1, card2) && KindMatches(card1, card2);
        }

        public static bool ContainsConsecutivePlaceCardAttempts(List<PlaceCardAttempt> placeCardAttempts)
        {
            var targetLaneIndexes = placeCardAttempts.Select(placeCardAttempt => placeCardAttempt.TargetLaneIndex).ToList();
            targetLaneIndexes.Sort();

            for (int i = 0; i < targetLaneIndexes.Count - 1; i++)
            {
                if (targetLaneIndexes[i + 1] - targetLaneIndexes[i] != 1)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TriedToCaptureDistantRow(Game game, Move move, bool playerIsHost)
        {
            var firstPlaceCardAttempt = move.PlaceCardAttempts.FirstOrDefault();
            if (firstPlaceCardAttempt is null)
            {
                return false;
            }

            if (playerIsHost)
            {
                var startIndex = StartedMovePlayerSide(game, move, playerIsHost) ? 0 : 4;
                return !CapturedAllPreviousRows(game, firstPlaceCardAttempt, startIndex, playerIsHost);
            }

            var endIndex = StartedMoveOpponentSide(game, move, playerIsHost) ? 2 : 6;
            return !CapturedAllFollowingRows(game, firstPlaceCardAttempt, endIndex, playerIsHost);
        }

        public static bool StartedMovePlayerSide(Game game, Move move, bool playerIsHost)
        {
            var firstPlaceCardAttempt = move.PlaceCardAttempts.FirstOrDefault();
            if (firstPlaceCardAttempt is null)
            {
                return false;
            }

            return playerIsHost
                ? firstPlaceCardAttempt.TargetRowIndex < 3
                : firstPlaceCardAttempt.TargetRowIndex > 3;
        }

        public static bool StartedMoveOpponentSide(Game game, Move move, bool playerIsHost)
        {
            var firstPlaceCardAttempt = move.PlaceCardAttempts.FirstOrDefault();
            if (firstPlaceCardAttempt is null)
            {
                return false;
            }

            return playerIsHost
                ? firstPlaceCardAttempt.TargetRowIndex > 3
                : firstPlaceCardAttempt.TargetRowIndex < 3;
        }

        public static bool CapturedAllPreviousRows(Game game, PlaceCardAttempt placeCardAttempt, int startIndex, bool playerIsHost)
        {
            var targetLaneIndex = placeCardAttempt.TargetLaneIndex;
            var targetRowIndex = placeCardAttempt.TargetRowIndex;
            var lane = game.Lanes[targetLaneIndex];

            for (int i = startIndex; i < targetRowIndex; i++)
            {
                var previousRow = lane.Rows[i];
                var previousRowNotOccupied = previousRow.Count == 0;

                if (previousRowNotOccupied)
                {
                    return false;
                }

                var topCard = previousRow[previousRow.Count - 1];
                var topCardPlayedByPlayer = playerIsHost
                    ? topCard.PlayedBy == PlayerOrNone.Host
                    : topCard.PlayedBy == PlayerOrNone.Guest;

                if (!topCardPlayedByPlayer)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CapturedAllFollowingRows(Game game, PlaceCardAttempt placeCardAttempt, int endIndex, bool playerIsHost)
        {
            var targetLaneIndex = placeCardAttempt.TargetLaneIndex;
            var targetRowIndex = placeCardAttempt.TargetRowIndex;
            var lane = game.Lanes[targetLaneIndex];

            for (int i = endIndex; i > targetRowIndex; i--)
            {
                var followingRow = lane.Rows[i];
                var followingRowNotOccupied = followingRow.Count == 0;

                if (followingRowNotOccupied)
                {
                    return false;
                }

                var topCard = followingRow[followingRow.Count - 1];
                var topCardPlayedByPlayer = playerIsHost
                    ? topCard.PlayedBy == PlayerOrNone.Host
                    : topCard.PlayedBy == PlayerOrNone.Guest;

                if (!topCardPlayedByPlayer)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TargetLaneHasBeenWon(Game game, Move move)
        {
            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
                if (lane.WonBy != PlayerOrNone.None)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TriedToCaptureGreaterCard(Game game, Move move, bool playerIsHost)
        {
            var firstPlaceCardAttempt = GetInitialPlaceCardAttempt(move, playerIsHost);
            var card = firstPlaceCardAttempt.Card;
            var targetLaneIndex = firstPlaceCardAttempt.TargetLaneIndex;
            var targetRowIndex = firstPlaceCardAttempt.TargetRowIndex;
            var targetRow = game.Lanes[targetLaneIndex].Rows[targetRowIndex];

            if (targetRow.Count <= 0)
            {
                return false;
            }

            var targetCard = targetRow[targetRow.Count - 1];
            var suitsMatch = targetCard.Suit == card.Suit;
            var targetCardIsGreater = !CardTrumpsCard(card, targetCard);

            return suitsMatch && targetCardIsGreater;
        }

        public static PlaceCardAttempt GetInitialPlaceCardAttempt(Move move, bool playerIsHost)
        {
            PlaceCardAttempt initialPlaceCardAttempt = move.PlaceCardAttempts[0];

            foreach(var placeCardAttempt in move.PlaceCardAttempts)
            {
                var isMoreInitial = playerIsHost
                    ? placeCardAttempt.TargetRowIndex < initialPlaceCardAttempt.TargetRowIndex
                    : placeCardAttempt.TargetRowIndex > initialPlaceCardAttempt.TargetRowIndex;

                if (isMoreInitial)
                {
                    initialPlaceCardAttempt = placeCardAttempt;
                }
            }

            return initialPlaceCardAttempt;
        }

        public static bool CardTrumpsCard(Card offender, Card defender)
        {
            return SuitMatches(offender, defender)
                ? offender.Kind > defender.Kind
                : KindMatches(offender, defender);
        }

        public static bool PlayerHasAdvantage(Game game, Move move, bool playerIsHost)
        {
            var targetLaneIndex = move.PlaceCardAttempts[0].TargetLaneIndex;
            return game.Lanes[targetLaneIndex].LaneAdvantage == (playerIsHost
                ? PlayerOrNone.Host
                : PlayerOrNone.Guest);
        }

        public static bool OpponentHasAdvantage(Game game, Move move, bool playerIsHost)
        {
            var targetLaneIndex = move.PlaceCardAttempts[0].TargetLaneIndex;
            return game.Lanes[targetLaneIndex].LaneAdvantage == (playerIsHost
                ? PlayerOrNone.Guest
                : PlayerOrNone.Host);
        }

        public static bool LaneHasNoAdvantage(Game game, Move move)
        {
            return game.Lanes[move.PlaceCardAttempts[0].TargetLaneIndex].LaneAdvantage == PlayerOrNone.None;
        }

        /// <summary>
        /// Returns true if the target lane has no last card played.
        /// </summary>
        public static bool SuitOrKindMatchesLastCardPlayed(Game game, Move move, bool playerIsHost)
        {
            var firstAttempt = GetInitialPlaceCardAttempt(move, playerIsHost);
            var lastCardPlayedInLane = game.Lanes[firstAttempt.TargetLaneIndex].LastCardPlayed;

            if (lastCardPlayedInLane is null)
            {
                return true;
            }

            return SuitMatches(firstAttempt.Card, lastCardPlayedInLane) 
                || KindMatches(firstAttempt.Card, lastCardPlayedInLane);
        }

        static bool TriedToReinforceGreaterCard(Game game, Move move, bool playerIsHost)
        {
            var firstAttempt = GetInitialPlaceCardAttempt(move, playerIsHost);
            var targetRow = game.Lanes[firstAttempt.TargetLaneIndex].Rows[firstAttempt.TargetRowIndex];
            var targetCard = targetRow.LastOrDefault();

            if (targetCard is null)
            {
                return false;
            }

            var playerPlayedTargetCard = playerIsHost
                ? targetCard.PlayedBy == PlayerOrNone.Host
                : targetCard.PlayedBy == PlayerOrNone.Guest;

            return playerPlayedTargetCard 
                && SuitMatches(targetCard, firstAttempt.Card) 
                && !CardTrumpsCard(targetCard, firstAttempt.Card);
        }
    }
}
