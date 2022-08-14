namespace LanesBackend.Models
{
    public class Card
    {
        public readonly string Kind;

        public readonly string Suit;

        public Card(string kind, string suit)
        {
            Kind = kind;
            Suit = suit;
        }
    }
}
