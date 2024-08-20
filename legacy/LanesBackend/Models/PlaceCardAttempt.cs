namespace LanesBackend.Models
{
  public class PlaceCardAttempt
  {
    public Card Card { get; set; }

    public int TargetLaneIndex { get; set; }

    public int TargetRowIndex { get; set; }

    public PlaceCardAttempt(Card card, int targetLaneIndex, int targetRowIndex)
    {
      Card = card;
      TargetLaneIndex = targetLaneIndex;
      TargetRowIndex = targetRowIndex;
    }
  }
}
