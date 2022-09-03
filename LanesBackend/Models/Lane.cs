namespace LanesBackend.Models
{
    public class Lane
    {
        public List<Card>[] Rows = new List<Card>[7];

        public LaneAdvantage LaneAdvantage = LaneAdvantage.None;

        public Card? LastCardPlayed;

        public Lane()
        {
            InitEmptyLanes();
        }

        /// <summary>
        /// Fetches all the cards from the lane and resets the lane data.
        /// </summary>
        public List<Card> GrabAllCards()
        {
            List<Card> cards = new List<Card>();

            foreach(var row in Rows)
            {
                foreach(var card in row)
                {
                    cards.Add(card);
                }
            }

            InitEmptyLanes();

            return cards;
        }

        private void InitEmptyLanes()
        {
            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new List<Card>();
            }
        }
    }
}
