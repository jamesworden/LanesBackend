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
    }
}
