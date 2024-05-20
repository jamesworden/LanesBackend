namespace LanesBackend.Models
{
    public class Card
    {
        public Kind Kind { get; set; }

        public Suit Suit { get; set; }

        public PlayerOrNone PlayedBy { get; set; }

        public Card(Kind kind, Suit suit, PlayerOrNone playedBy = PlayerOrNone.None)
        {
            Kind = kind;
            Suit = suit;
            PlayedBy = playedBy;
        }
    }
}
