namespace LanesBackend.Models.GameEngine
{
    public class AlgoPlaceCardAttempt
    {
        public AlgoCard Card { get; set; }

        public int TargetLaneIndex { get; set; }

        public int TargetRowIndex { get; set; }

        public AlgoPlaceCardAttempt(AlgoCard card, int targetLaneIndex, int targetRowIndex)
        {
            Card = card;
            TargetLaneIndex = targetLaneIndex;
            TargetRowIndex = targetRowIndex;
        }
    }
}
