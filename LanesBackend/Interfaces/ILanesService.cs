using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface ILanesService
    {
        public Lane[] CreateEmptyLanes();

        public List<Card> GrabAllCardsAndClearLane(Lane lane);
    }
}
