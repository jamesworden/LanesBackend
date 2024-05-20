namespace LanesBackend.Models
{
    public class Deck
    {
        public List<Card> Cards { get; set; }

        public Deck(List<Card> cards)
        {
            Cards = cards;
        }
    }
}
