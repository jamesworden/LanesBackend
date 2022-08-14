using LanesBackend.Models;

namespace LanesBackend.CacheModels
{
    public class Game
    {
        public bool isRunning = true;

        public string HostConnectionId { get; set; }

        public string GuestConnectionId { get; set; }

        public string GameCode { get; set; }

        public Lane[] Lanes = new Lane[5];

        public Game(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            HostConnectionId = hostConnectionId;
            GuestConnectionId = guestConnectionId;
            GameCode = gameCode;
            
            for (int i = 0; i < Lanes.Length; i++)
            {
                Lanes[i] = new Lane();
            }
        }

    }
}
