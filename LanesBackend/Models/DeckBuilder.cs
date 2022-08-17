namespace LanesBackend.Models
{
    public class DeckBuilder
    {
        private List<Card> Cards;

        public DeckBuilder()
        {
            this.Cards = new List<Card>();
        }

        public DeckBuilder FillWithCards()
        {
            Cards = new List<Card>();

            var suits = Enum.GetValues(typeof(Suit));

            foreach (Suit suit in suits)
            {
                var kinds = Enum.GetValues(typeof(Kind));

                foreach (Kind kind in kinds)
                {
                    var card = new Card(kind, suit);
                    Cards.Add(card);
                }
            }

            return this;
        }

        public Deck Build()
        {
            return new Deck(Cards);
        }
    }
}