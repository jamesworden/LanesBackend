namespace LanesBackend.Models
{
  /// <summary>
  /// Where a card is in the game. Only one property should be truthy.
  /// </summary>
  public class CardStore
  {
    public int? HostHandCardIndex { get; set; } = null;

    public int? GuestHandCardIndex { get; set; } = null;

    public CardPosition? CardPosition { get; set; } = null;

    public bool Destroyed = false;

    public bool HostDeck = false;

    public bool GuestDeck = false;

    public CardStore() { }
  }
}
