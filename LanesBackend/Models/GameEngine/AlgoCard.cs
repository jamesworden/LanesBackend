namespace LanesBackend.Models.GameEngine
{
    public class AlgoCard
    {
        public readonly Kind Kind;

        public readonly Suit Suit;

        public AlgoPlayer PlayedBy = AlgoPlayer.None;

        public AlgoCard(Kind kind, Suit suit)
        {
            Kind = kind;
            Suit = suit;
        }
    }
}
