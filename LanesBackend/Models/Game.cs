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

            var moveWasValid = false;

            LaneUtils.ModifyMoveFromHostPov(move, playerIsHost, (hostPovMove) =>
            {
                LaneUtils.ModifyLaneFromHostPov(targetLane, playerIsHost, (hostPovLane) =>
                {
                    var moveIsValid = MoveChecker.IsMoveValidFromHostPov(hostPovMove, hostPovLane);

                    if (moveIsValid)
                    {
                        MakeMoveFromHostPov(move, targetLane, playerIsHost);
                        moveWasValid = true;
                    }
                });
            });

            return moveWasValid;
        }

        private void MakeMoveFromHostPov(Move move, Lane targetLane, bool playerIsTruelyHost)
        {
            var isPairMove = move.PlaceCardAttempts.Count > 1;

            if (!isPairMove)
            {
                IsHostPlayersTurn = !IsHostPlayersTurn;
            }

            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                var aceRuleTriggered = TriggerAceRuleIfAppropriate(placeCardAttempt, targetLane);

                if (aceRuleTriggered)
                {
                    return;
                }

                var targetRow = targetLane.Rows[placeCardAttempt.TargetRowIndex];
                placeCardAttempt.Card.PlayedBy = playerIsTruelyHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
                targetRow.Add(placeCardAttempt.Card);

                targetLane.LastCardPlayed = placeCardAttempt.Card;

                var middleCaptured = CaptureMiddleIfAppropriate(placeCardAttempt, targetLane, playerIsTruelyHost);

                if (middleCaptured)
                {
                    return;
                }

                _ = WinLaneIfAppropriate(placeCardAttempt, targetLane);
            }
        }

        /// <returns>True if the middle was captured, false if not.</returns>
        private bool CaptureMiddleIfAppropriate(PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsTruelyHost)
        {
            var cardIsLastOnHostSide = placeCardAttempt.TargetRowIndex == 2;

            if (!cardIsLastOnHostSide)
            {
                return false;
            }

            if (lane.LaneAdvantage == PlayerOrNone.None)
            {
                CaptureNoAdvantageLane(lane, placeCardAttempt);
            }
            else if (lane.LaneAdvantage == PlayerOrNone.Guest)
            {
                CaptureOpponentAdvantageLane(lane, playerIsTruelyHost);
            }

            return true;
        }

        private void CaptureNoAdvantageLane(Lane lane, PlaceCardAttempt placeCardAttempt)
        {
            var cardsFromLane = lane.GrabAllCards();
            // Put last placed card at top of pile
            cardsFromLane.Remove(placeCardAttempt.Card);
            cardsFromLane.Add(placeCardAttempt.Card);
            var middleRow = lane.Rows[3];
            middleRow.AddRange(cardsFromLane);

            lane.LaneAdvantage = PlayerOrNone.Host;
        }

        private void CaptureOpponentAdvantageLane(Lane lane, bool playerIsTruelyHost)
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

            var player = playerIsTruelyHost ? HostPlayer : GuestPlayer;
            player.Deck.Cards.AddRange(remainingCardsInLane);
            player.Deck.Shuffle();

            lane.LaneAdvantage = PlayerOrNone.Host;
        }

        private bool TriggerAceRuleIfAppropriate(PlaceCardAttempt placeCardAttempt, Lane lane)
        {
            var playerPlayedAnAce = placeCardAttempt.Card.Kind == Kind.Ace;

            if (!playerPlayedAnAce)
            {
                return false;
            }

            var opponentAceOnTopCardOfAnyRow = false;

            for (int i = 0; i < lane.Rows.Length; i++)
            {
                var topCard = LaneUtils.GetTopCardOfTargetRow(lane, i);

                if (topCard is null)
                {
                    continue;
                }

                if (topCard.PlayedBy == PlayerOrNone.Guest && topCard.Kind == Kind.Ace)
                {
                    opponentAceOnTopCardOfAnyRow = true;
                    break;
                }
            }

            if (!opponentAceOnTopCardOfAnyRow)
            {
                return false;
            }

            _ = lane.GrabAllCards();

            return true;
        }

        private bool WinLaneIfAppropriate(PlaceCardAttempt placeCardAttempt, Lane lane)
        {
            var placeCardInLastRow = placeCardAttempt.TargetRowIndex == 6;

            if (!placeCardInLastRow)
            {
                return false;
            }

            lane.WonBy = PlayerOrNone.Host;
            _ = lane.GrabAllCards();
            // Joker?
            return true;
        }
    }
}