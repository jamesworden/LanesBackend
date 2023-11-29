namespace LanesBackend.Models
{
    public class MoveMade
    {
        public PlayerOrNone PlayedBy { get; set; }

        public Move Move { get; set; }

        public DateTime TimestampUTC { get; set; }

        public List<CardMovement> CardMovements { get; set; }

    public MoveMade(PlayerOrNone playedBy, Move move, DateTime timestampUTC, List<CardMovement> cardMovements)
        {
            PlayedBy = playedBy;
            Move = move;
            TimestampUTC = timestampUTC;
            CardMovements = cardMovements;
        }
    }
}
