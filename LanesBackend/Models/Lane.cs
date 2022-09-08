namespace LanesBackend.Models
{
    public class Lane
    {
        public List<Card>[] Rows = new List<Card>[7];

        public PlayerOrNone LaneAdvantage = PlayerOrNone.None;

        public Card? LastCardPlayed;

        public PlayerOrNone WonBy = PlayerOrNone.None;

        public Lane(List<Card>[] rows)
        {
            Rows = rows;
        }
    }
}
