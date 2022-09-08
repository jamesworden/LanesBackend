namespace LanesBackend.Models
{
    public class Deck
    {

        public List<Card> Cards;

        public Deck(List<Card> cards)
        {
            Cards = cards;
        }
    }
}