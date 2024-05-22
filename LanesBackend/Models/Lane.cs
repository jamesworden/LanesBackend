namespace LanesBackend.Models
{
  public class Lane
  {
    public List<Card>[] Rows { get; set; } = new List<Card>[7];

    public PlayerOrNone LaneAdvantage { get; set; } = PlayerOrNone.None;

    public Card? LastCardPlayed { get; set; }

    public PlayerOrNone WonBy { get; set; } = PlayerOrNone.None;

    public Lane(List<Card>[] rows)
    {
      Rows = rows;
    }
  }
}
