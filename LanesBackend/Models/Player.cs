namespace LanesBackend.Models
{
    public class Player
    {
        public Hand Hand { get; set; }
        public Deck Deck { get; set; }
        public string Name { get; set; }

        public Player(List<Card> initialCards, string name)
        {
            Deck = new Deck(initialCards);
            var fiveCards = Deck.DrawCards(5);
            Hand = new Hand(fiveCards);
            Name = name;
        }
    }
}
