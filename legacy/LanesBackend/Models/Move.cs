namespace LanesBackend.Models
{
  public class Move
  {
    public List<PlaceCardAttempt> PlaceCardAttempts { get; set; }

    public Move(List<PlaceCardAttempt> placeCardAttempts)
    {
      PlaceCardAttempts = placeCardAttempts;
    }
  }
}
