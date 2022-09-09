namespace LanesBackend.Models.GameEngine
{
    public class AlgoLane
    {
        public List<AlgoCard>[] Rows = new List<AlgoCard>[7];

        public AlgoPlayer LaneAdvantage = AlgoPlayer.None;

        public AlgoCard? LastCardPlayed;

        public AlgoPlayer WonBy = AlgoPlayer.None;

        public AlgoLane(List<AlgoCard>[] rows)
        {
            Rows = rows;
        }
    }
}
