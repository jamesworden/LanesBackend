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

        public Game(
            string hostConnectionId, 
            string guestConnectionId, 
            string gameCode, 
            Player hostPlayer,
            Player guestPlayer,
            Lane[] lanes)
        {
            HostConnectionId = hostConnectionId;
            GuestConnectionId = guestConnectionId;
            GameCode = gameCode;
            HostPlayer = hostPlayer;
            GuestPlayer = guestPlayer;
            Lanes = lanes;
        }
    }
}