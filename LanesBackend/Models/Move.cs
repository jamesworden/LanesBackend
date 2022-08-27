namespace LanesBackend.Models
{
    public class Move
    {
        public Card Card { get; set; }

        public int TargetLaneIndex { get; set; }

        public int TargetRowIndex { get; set; }

        public Move(Card card, int targetLaneIndex, int targetRowIndex)
        {
            Card = card;
            TargetLaneIndex = targetLaneIndex;
            TargetRowIndex = targetRowIndex;
        }
    }
}
