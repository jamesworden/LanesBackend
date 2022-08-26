namespace LanesBackend.Models
{
    public class Card : IComparable
    {
        public readonly Kind Kind;

        public readonly Suit Suit;

        public Card(Kind kind, Suit suit)
        {
            Kind = kind;
            Suit = suit;
        }

        public int CompareTo(object? obj)
        {
            if (obj == null)
            {
                return 1;
            }

            var card = obj as Card;

            if (card == null)
            {
                return 1;
            }

            return Kind.CompareTo(card.Kind);
        }
    }
}
