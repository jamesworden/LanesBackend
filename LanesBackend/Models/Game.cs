using LanesBackend.Models;
using LanesBackend.Utils;

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
        public bool MakeMoveIfValid(Move move, string playerConnectionId)
        {
            var playerIsHost = HostConnectionId == playerConnectionId;
            var isPlayersTurn = playerIsHost && IsHostPlayersTurn || !playerIsHost && !IsHostPlayersTurn;
             
            if (!isPlayersTurn)
            {
                return false;
            }

            // Move should contain place card attempts only for one specific lane.
            var targetLane = Lanes[move.PlaceCardAttempts[0].TargetLaneIndex];

            var moveIsValid = MoveChecker.IsMoveValid(move, targetLane, playerIsHost);

            if (moveIsValid)
            {
                MakeMove(move, playerIsHost, targetLane);
            }

            return moveIsValid;
        }

        private void MakeMove(Move move, bool playerIsHost, Lane targetLane)
        {
            var isPairMove = move.PlaceCardAttempts.Count > 1;

            if (!isPairMove)
            {
                IsHostPlayersTurn = !IsHostPlayersTurn;
            }

            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                placeCardAttempt.Card.PlayedBy = playerIsHost ? PlayedBy.Host : PlayedBy.Guest;

                var targetRow = targetLane.Rows[placeCardAttempt.TargetRowIndex];
                targetRow.Add(placeCardAttempt.Card);

                targetLane.LastCardPlayed = placeCardAttempt.Card;

                // CaptureMiddleIfAppropriate(placeCardAttempt, targetLane, playerIsHost);
            }
        }

        private void CaptureMiddleIfAppropriate(PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsHost)
        {
            var cardIsLastOnHostSide = placeCardAttempt.TargetRowIndex == 2;
            var hostShouldCaptureMiddle = playerIsHost && cardIsLastOnHostSide;

            if (hostShouldCaptureMiddle)
            {
                if (lane.LaneAdvantage == LaneAdvantage.None)
                {
                    var cardsFromLane = lane.GrabAllCards();
                    // Put last placed card at top of pile
                    cardsFromLane.Remove(placeCardAttempt.Card);
                    cardsFromLane.Add(placeCardAttempt.Card);
                    lane.Rows[3].AddRange(cardsFromLane);
                }

                //      If there is an opponent advantage - gather top three cards from player side
                //      Comment with don't worry about playerside advantage because you can't move on playerside if there's a player advantage.
            }

            var cardIsLastOnGuestSide = placeCardAttempt.TargetRowIndex == 4;
            var guestShouldCaptureMiddle = !playerIsHost && cardIsLastOnGuestSide;

            if (guestShouldCaptureMiddle)
            {
                //      If no advantage - gather all cards from lane into new list - put that list of cards in the middle with the top card as the card that is played
                //      If there is an opponent advantage - gather top three cards from player side
                //      Comment with don't worry about playerside advantage because you can't move on playerside if there's a player advantage.
                return;
            }
        }
    }
}
