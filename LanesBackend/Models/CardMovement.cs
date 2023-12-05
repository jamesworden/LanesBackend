namespace LanesBackend.Models
{
    public class CardMovement
    {
       public CardStore From { get; set; }

        public CardStore To { get; set; }

        public Card? Card { get; set; }

        public CardMovement(CardStore from, CardStore to, Card? card)
        {
            From = from;
            To = to;
            Card = card;
        }
    }
}
