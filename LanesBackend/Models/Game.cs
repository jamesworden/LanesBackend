namespace LanesBackend.Models
{
    public class Game
    {
        public bool isRunning = true;

        public PlayerOrNone WonBy = PlayerOrNone.None;

        public bool IsHostPlayersTurn = true;

        public string HostConnectionId { get; set; }

        public string GuestConnectionId { get; set; }

        public string GameCode { get; set; }

        public Lane[] Lanes = new Lane[5];

        public Player HostPlayer { get; set; }

        public Player GuestPlayer { get; set; }

        public int? RedJokerLaneIndex { get; set; }

        public int? BlackJokerLaneIndex { get; set; }

        public DateTime GameCreatedTimestampUTC { get; set; }

        public List<MoveMade> MovesMade = new();

        public DurationOption DurationOption { get; set; }

        public DateTime? GameEndedTimestampUTC { get; set; }

        public List<List<CandidateMove>> CandidateMoves { get; set; } = new();

        public Game(
            string hostConnectionId, 
            string guestConnectionId, 
            string gameCode, 
            Player hostPlayer,
            Player guestPlayer,
            Lane[] lanes,
            DateTime gameCreatedTimestampUTC,
            DurationOption durationOption,
            DateTime? gameEndedTimestampUTC = null)
        {
            HostConnectionId = hostConnectionId;
            GuestConnectionId = guestConnectionId;
            GameCode = gameCode;
            HostPlayer = hostPlayer;
            GuestPlayer = guestPlayer;
            Lanes = lanes;
            GameCreatedTimestampUTC = gameCreatedTimestampUTC;
            DurationOption = durationOption;
            GameEndedTimestampUTC = gameEndedTimestampUTC;
        }
    }
}