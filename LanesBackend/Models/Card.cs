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

        // TODO: I don't think compare to should be how we compare which card is valid
        // because when battling, a king can be placed on top of any other king
        // - I guess we can modify it to reflect that, but why not just call a function
        // like AttemptToCapture() or something...
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
