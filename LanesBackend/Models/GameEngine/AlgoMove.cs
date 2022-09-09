namespace LanesBackend.Models.GameEngine
{
    public class AlgoMove
    {
        public List<AlgoPlaceCardAttempt> PlaceCardAttempts { get; set; }

        public AlgoMove(List<AlgoPlaceCardAttempt> placeCardAttempts)
        {
            PlaceCardAttempts = placeCardAttempts;
        }
    }
}
