namespace LanesBackend.Models
{
  public class Player
  {
    public Hand Hand { get; set; }

    public Deck Deck { get; set; }

    public Player(Deck deck, Hand hand)
    {
      Deck = deck;
      Hand = hand;
    }
  }
}
