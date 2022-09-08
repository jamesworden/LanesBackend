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

        /// <summary>
        /// Fetches all the cards from the lane and resets the lane data.
        /// </summary>
        public List<Card> GrabAllCards()
        {
            List<Card> cards = new();

            foreach(var row in Rows)
            {
                foreach(var card in row)
                {
                    cards.Add(card);
                }
            }

            // REFACTOR DEBT: INIT EMPTY LANES!

            return cards;
        }
    }
}
