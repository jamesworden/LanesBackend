namespace LanesBackend.Models
{
    public class CardPosition
    {
        public int LaneIndex { get; set; }

        public int RowIndex { get; set; }

        public CardPosition(int laneIndex, int rowIndex)
        {
            LaneIndex = laneIndex;
            RowIndex = rowIndex;
        }
    }
}
