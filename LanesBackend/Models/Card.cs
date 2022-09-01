namespace LanesBackend.Models
{
    public class Card
    {
        public readonly Kind Kind;

        public readonly Suit Suit;

        public PlayedBy PlayedBy = PlayedBy.NotYetPlayed;

        public Card(Kind kind, Suit suit)
        {
            Kind = kind;
            Suit = suit;
        }
    }
}
