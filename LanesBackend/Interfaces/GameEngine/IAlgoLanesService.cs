using LanesBackend.Models.GameEngine;

namespace LanesBackend.Interfaces
{
    public interface IAlgoLanesService
    {
        public List<AlgoCard> GrabAllCardsAndClearLane(AlgoLane lane);
    }
}
