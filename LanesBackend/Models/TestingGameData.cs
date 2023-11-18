namespace LanesBackend.Models
{
    public class TestingGameData
    {
        public Lane[] Lanes { get; set; }

        public Hand HostHand { get; set; }

        public Hand GuestHand { get; set; }

        public Deck HostDeck { get; set; }

        public Deck GuestDeck { get; set; }

        public int? RedJokerLaneIndex { get; set; }

        public int? BlackJokerLaneIndex { get; set; }

        public bool IsHostPlayersTurn { get; set; }

        public TestingGameData(Lane[] lanes, Hand hostHand, Hand guestHand, Deck hostDeck, Deck guestDeck, int? redJokerLaneIndex, int? blackJokerLaneIndex) { 
            Lanes = lanes;
            HostHand = hostHand;
            GuestHand = guestHand;
            HostDeck = hostDeck;
            GuestDeck = guestDeck;
            RedJokerLaneIndex = redJokerLaneIndex;
            BlackJokerLaneIndex= blackJokerLaneIndex;
        }
    }
}
