using LanesBackend.Models;

namespace LanesBackend.Interfaces.GameEngine
{
    public interface IGameRuleService
    {
        public bool CaptureMiddleIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost);

        public bool TriggerAceRuleIfAppropriate(Lane lane, PlaceCardAttempt placeCardAttempt, bool playerIsHost);

        public bool WinLaneIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost);

        public bool WinGameIfAppropriate(Game game);
    }
}
