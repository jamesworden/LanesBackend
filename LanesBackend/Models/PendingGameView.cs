namespace LanesBackend.Models
{
    public class PendingGameView
    {
        public string GameCode { get; set; }

        public DurationOption DurationOption { get; set; }

        public PendingGameView(string gameCode, DurationOption durationOption)
        {
            GameCode = gameCode;
            DurationOption = durationOption;
        }
    }
}
