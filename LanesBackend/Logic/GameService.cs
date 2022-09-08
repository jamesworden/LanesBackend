using LanesBackend.Interfaces;
using LanesBackend.Models;
using LanesBackend.Utils;

namespace LanesBackend.Logic
{
    public class GameService : IGameService
    {
        private readonly IDeckService DeckService;

        private readonly ILanesService LanesService;

        public GameService(IDeckService deckService, ILanesService lanesService)
        {
            DeckService = deckService;
            LanesService = lanesService;
        }

        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            var deck = DeckService.CreateAndShuffleDeck();
            var playerDecks = DeckService.SplitDeck(deck);

            var hostDeck = playerDecks.Item1;
            var guestDeck = playerDecks.Item2;

            var hostHandCards = DeckService.DrawCards(hostDeck, 5);
            var guestHandCards = DeckService.DrawCards(guestDeck, 5);

            var hostHand = new Hand(hostHandCards);
            var guestHand = new Hand(guestHandCards);

            var hostPlayer = new Player(hostDeck, hostHand);
            var guestPlayer = new Player(guestDeck, guestHand);

            var lanes = LanesService.CreateEmptyLanes();

            Game game = new(hostConnectionId, guestConnectionId, gameCode, hostPlayer, guestPlayer, lanes);

            return game;
        }

        public bool MakeMoveIfValid(Game game, Move move, string playerConnectionId)
        {
            var playerIsHost = game.HostConnectionId == playerConnectionId;
            var hostAndHostTurn = playerIsHost && game.IsHostPlayersTurn;
            var guestAndGuestTurn = !playerIsHost && !game.IsHostPlayersTurn;
            var isPlayersTurn = hostAndHostTurn || guestAndGuestTurn;

            if (!isPlayersTurn)
            {
                return false;
            }

            // Move should contain place card attempts only for one specific lane.
            var targetLane = game.Lanes[move.PlaceCardAttempts[0].TargetLaneIndex];

            var moveWasValid = false;

            LaneUtils.ModifyMoveFromHostPov(move, playerIsHost, (hostPovMove) =>
            {
                LaneUtils.ModifyLaneFromHostPov(targetLane, playerIsHost, (hostPovLane) =>
                {
                    var moveIsValid = MoveChecker.IsMoveValidFromHostPov(hostPovMove, hostPovLane);

                    if (moveIsValid)
                    {
                        MakeMoveFromHostPov(game, hostPovMove, hostPovLane, playerIsHost);
                        moveWasValid = true;
                    }
                });
            });

            return moveWasValid;
        }

        private void MakeMoveFromHostPov(Game game, Move hostPovMove, Lane hostPovLane, bool playerIsTruelyHost)
        {
            var isPairMove = hostPovMove.PlaceCardAttempts.Count > 1;

            if (!isPairMove)
            {
                game.IsHostPlayersTurn = !game.IsHostPlayersTurn;
            }

            foreach (var placeCardAttempt in hostPovMove.PlaceCardAttempts)
            {
                PlaceCardFromHostPovAndApplyGameRules(game, placeCardAttempt, hostPovLane, playerIsTruelyHost);
            }
        }

        private void PlaceCardFromHostPovAndApplyGameRules(Game game, PlaceCardAttempt placeCardAttempt, Lane targetLane, bool playerIsTruelyHost)
        {
            var aceRuleTriggered = TriggerAceRuleIfAppropriateFromHostPov(placeCardAttempt, targetLane);

            if (aceRuleTriggered)
            {
                return;
            }

            PlaceCardFromHostPov(targetLane, placeCardAttempt);

            var middleCaptured = CaptureMiddleIfAppropriateFromHostPov(game, placeCardAttempt, targetLane, playerIsTruelyHost);

            if (middleCaptured)
            {
                return;
            }

            _ = WinLaneIfAppropriateFromHostPov(game, placeCardAttempt, targetLane, playerIsTruelyHost);
        }

        private void PlaceCardFromHostPov(Lane lane, PlaceCardAttempt placeCardAttempt)
        {
            var targetRow = lane.Rows[placeCardAttempt.TargetRowIndex];
            placeCardAttempt.Card.PlayedBy = PlayerOrNone.Host;
            targetRow.Add(placeCardAttempt.Card);
            lane.LastCardPlayed = placeCardAttempt.Card;
        }

        private bool CaptureMiddleIfAppropriateFromHostPov(Game game, PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsTruelyHost)
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
                CaptureOpponentAdvantageLane(game, lane, playerIsTruelyHost);
            }

            return true;
        }

        private void CaptureNoAdvantageLane(Lane lane, PlaceCardAttempt placeCardAttempt)
        {
            var cardsFromLane = LanesService.GrabAllCardsAndClearLane(lane);

            // Put last placed card at top of pile
            cardsFromLane.Remove(placeCardAttempt.Card);
            cardsFromLane.Add(placeCardAttempt.Card);

            var middleRow = lane.Rows[3];
            middleRow.AddRange(cardsFromLane);
            lane.LaneAdvantage = PlayerOrNone.Host;
        }

        private void CaptureOpponentAdvantageLane(Game game, Lane lane, bool playerIsTruelyHost)
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

            var remainingCardsInLane = LanesService.GrabAllCardsAndClearLane(lane);

            var middleRow = lane.Rows[3];
            middleRow.AddRange(topCardsOfFirstThreeRows);

            var player = playerIsTruelyHost ? game.HostPlayer : game.GuestPlayer;
            player.Deck.Cards.AddRange(remainingCardsInLane);

            DeckService.ShuffleDeck(player.Deck);

            lane.LaneAdvantage = PlayerOrNone.Host;
        }

        private bool TriggerAceRuleIfAppropriateFromHostPov(PlaceCardAttempt placeCardAttempt, Lane lane)
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

            _ = LanesService.GrabAllCardsAndClearLane(lane);

            return true;
        }

        private bool WinLaneIfAppropriateFromHostPov(Game game, PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsTruelyHost)
        {
            var placeCardInLastRow = placeCardAttempt.TargetRowIndex == 6;

            if (!placeCardInLastRow)
            {
                return false;
            }

            lane.WonBy = PlayerOrNone.Host;
            var allCardsInLane = LanesService.GrabAllCardsAndClearLane(lane);
            var player = playerIsTruelyHost ? game.HostPlayer : game.GuestPlayer;
            player.Deck.Cards.AddRange(allCardsInLane);
            DeckService.ShuffleDeck(player.Deck);
            // TODO: Add joker to lane?
            return true;
        }
    }
}
