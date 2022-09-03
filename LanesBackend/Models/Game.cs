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

                CaptureMiddleIfAppropriate(placeCardAttempt, targetLane, playerIsHost);
            }
        }

        private void CaptureMiddleIfAppropriate(PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsHost)
        {
            LaneUtils.ModifyLaneFromHostPov(lane, playerIsHost, (hostPovLane) =>
            {
                LaneUtils.ModifyPlaceCardAttemptFromHostPov(placeCardAttempt, playerIsHost, (placeCardAttemptHostPov) =>
                {
                    var cardIsLastOnHostSide = placeCardAttempt.TargetRowIndex == 2;

                    if (!cardIsLastOnHostSide)
                    {
                        return;
                    }

                    if (lane.LaneAdvantage == LaneAdvantage.None)
                    {
                        CaptureNoAdvantageLane(lane, placeCardAttempt);
                        return;
                    }

                    if (lane.LaneAdvantage == LaneAdvantage.Guest)
                    {
                        CaptureOpponentAdvantageLane(lane, playerIsHost);
                    }
                });
            });
        }

        private void CaptureNoAdvantageLane(Lane lane, PlaceCardAttempt placeCardAttempt)
        {
            var cardsFromLane = lane.GrabAllCards();
            // Put last placed card at top of pile
            cardsFromLane.Remove(placeCardAttempt.Card);
            cardsFromLane.Add(placeCardAttempt.Card);
            var middleRow = lane.Rows[3];
            middleRow.AddRange(cardsFromLane);

            lane.LaneAdvantage = LaneAdvantage.Host;
        }

        private void CaptureOpponentAdvantageLane(Lane lane, bool playerIsHost)
        {
            List<Card> topCardsOfFirstThreeRows = new();

            for (int i = 0; i < 3; i++)
            {
                var card = lane.Rows[i].TakeLast(1).FirstOrDefault();

                if (card is not null)
                {
                    topCardsOfFirstThreeRows.Add(card);
                }
            }

            var remainingCardsInLane = lane.GrabAllCards();

            var middleRow = lane.Rows[3];
            middleRow.AddRange(topCardsOfFirstThreeRows);

            // This updates the Game State without any transformations with the modify functions so it needs
            // to know exactly whose deck to add cards to
            var player = playerIsHost ? HostPlayer : GuestPlayer;

            player.Deck.Cards.AddRange(remainingCardsInLane);
            player.Deck.Shuffle();

            // This lane advantage goes to the current player; if guest, it will be transformed
            // to guest in the modify function
            lane.LaneAdvantage = LaneAdvantage.Host;
        }
    }
}