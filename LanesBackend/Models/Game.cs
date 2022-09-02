using LanesBackend.Models;
using Newtonsoft.Json;

namespace LanesBackend.CacheModels
{
    public class Game
    {
        public bool isRunning = true;

        public bool IsHostPlayersTurn = true;

        public string HostConnectionId { get; set; }

        public string GuestConnectionId { get; set; }

        public string GameCode { get; set; }

        public Lane[] Lanes = new Lane[5];

        public Player HostPlayer { get; set; }

        public Player GuestPlayer { get; set; }

        public Game(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            HostConnectionId = hostConnectionId;
            GuestConnectionId = guestConnectionId;
            GameCode = gameCode;
            
            for (int i = 0; i < Lanes.Length; i++)
            {
                Lanes[i] = new Lane();
            }

            var deck = new DeckBuilder().FillWithCards().Build();
            deck.Shuffle();

            var hostPlayerDeck = deck.DrawCards(26);
            var guestPlayerDeck = deck.DrawRemainingCards();

            HostPlayer = new Player(hostPlayerDeck, "Host Player");
            GuestPlayer = new Player(guestPlayerDeck, "Guest Player");
        }

        /// <returns>
        /// True if the move was valid and the game was updated;
        /// False if the move was invalid.
        /// </returns>
        public bool AttemptMove(Move move, string playerConnectionId)
        {
            var playerIsHost = HostConnectionId == playerConnectionId;
            var isPlayersTurn = playerIsHost && IsHostPlayersTurn || !playerIsHost && !IsHostPlayersTurn;
             
            if (!isPlayersTurn)
            {
                return false;
            }

            var isPairMove = move.PlaceCardAttempts.Count > 1;

            if (!isPairMove)
            {
                IsHostPlayersTurn = !IsHostPlayersTurn;
            }

            // Move should contain place card attempts only for one specific lane.
            var targetLane = Lanes[move.PlaceCardAttempts[0].TargetLaneIndex];

            if (!IsMoveValid(move, targetLane, playerIsHost))
            {
                return false;
            }

            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                placeCardAttempt.Card.PlayedBy = playerIsHost ? PlayedBy.Host : PlayedBy.Guest;

                var targetRow = targetLane.Rows[placeCardAttempt.TargetRowIndex];
                targetRow.Add(placeCardAttempt.Card);

                targetLane.LastCardPlayed = placeCardAttempt.Card;
            }

            return true;
        }

        public bool IsMoveValid(Move move, Lane lane, bool playerIsHost)
        {
            var clonedMove = JsonConvert.DeserializeObject<Move>(JsonConvert.SerializeObject(move));
            var clonedLane = JsonConvert.DeserializeObject<Lane>(JsonConvert.SerializeObject(lane));

            if (clonedMove == null || clonedLane == null)
            {
                throw new Exception("Error cloning move and lane in IsMoveValid()");
            }

            if (!playerIsHost)
            {
                ConvertMoveToHostPov(clonedMove);
                ConvertLaneToHostPov(clonedLane);
            }

            return IsMoveValidFromHostPov(clonedMove, clonedLane);
        }

        public void ConvertMoveToHostPov(Move move)
        {
            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                placeCardAttempt.TargetLaneIndex = 4 - placeCardAttempt.TargetLaneIndex;
                placeCardAttempt.TargetRowIndex = 6 - placeCardAttempt.TargetRowIndex;
            }
        }

        public void ConvertLaneToHostPov(Lane lane)
        {
            lane.Rows.Reverse();

            foreach(var row in lane.Rows)
            {
                foreach(var card in row)
                {
                    SwitchHostAndGuestPlayedBy(card);
                }
            }

            if (lane.LastCardPlayed != null)
            {
                SwitchHostAndGuestPlayedBy(lane.LastCardPlayed);
            }
        }

        public void SwitchHostAndGuestPlayedBy(Card card)
        {
            card.PlayedBy = card.PlayedBy == PlayedBy.Host ? PlayedBy.Guest : PlayedBy.Host;
        }

        private bool IsMoveValidFromHostPov(Move move, Lane lane)
        {
            // TODO
            return true;
        }
    }
}
