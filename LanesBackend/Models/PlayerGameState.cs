namespace LanesBackend.Models
{
    public class PlayerGameState
    {
        public int NumCardsInOpponentsHand { get; set; }

        public int NumCardsInOpponentsDeck { get; set; }

        public int NumCardsInPlayersDeck { get; set; }

        public Hand Hand { get; set; }

        public Lane[] Lanes { get; set; }

        public bool IsHost { get; set; }

        public PlayerGameState(
            int numCardsInOpponentsDeck,
            int numCardsInOpponentsHand,
            int numCardsInPlayersDeck,
            Hand hand,
            Lane[] lanes,
            bool isHost)
        {
            NumCardsInOpponentsDeck = numCardsInOpponentsDeck;
            NumCardsInOpponentsHand = numCardsInOpponentsHand;
            NumCardsInPlayersDeck = numCardsInPlayersDeck;
            Hand = hand;
            Lanes = lanes;
            IsHost = isHost;
        }
    }
}
