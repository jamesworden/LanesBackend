namespace LanesBackend.Models
{
    public class Deck
    {

        public List<Card> Cards;

        public Deck(List<Card> cards)
        {
            Cards = cards;
        }

        public void Shuffle()
        {
            Random random = new();
            Cards = Cards.OrderBy(card => random.Next()).ToList();
        }

        public List<Card> DrawRemainingCards()
        {
            List<Card> remainingCards = new(Cards);
            Cards.Clear();
            return remainingCards;
        }

        public Card? DrawCard()
        {
            List<Card> singleCardList = this.DrawCards(1);

            if (singleCardList.Count != 1)
            {
                return null;
            }

            return singleCardList.ElementAt(0);
        }

        public List<Card> DrawCards(int numberOfCards)
        {
            if (Cards.Count < numberOfCards)
            {
                return DrawRemainingCards();
            }

            List<Card> cardsDrawn = new();

            int topCardIndex = Cards.Count - 1;
            for (int i = topCardIndex; i > topCardIndex - numberOfCards; i--)
            {
                Card card = Cards.ElementAt(i);
                Cards.RemoveAt(i);
                cardsDrawn.Add(card);
            }

            return cardsDrawn;
        }
    }
}