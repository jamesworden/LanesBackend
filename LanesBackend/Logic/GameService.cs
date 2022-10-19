using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class GameService : IGameService
    {
        private readonly ILanesService LanesService;

        private readonly ICardService CardService;

        public GameService(
            ILanesService lanesService,
            ICardService cardService)
        {
            LanesService = lanesService;
            CardService = cardService;
         }

        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            var deck = CardService.CreateAndShuffleDeck();
            var playerDecks = CardService.SplitDeck(deck);

            var hostDeck = playerDecks.Item1;
            var guestDeck = playerDecks.Item2;

            var hostHandCards = CardService.DrawCards(hostDeck, 5);
            var guestHandCards = CardService.DrawCards(guestDeck, 5);

            var hostHand = new Hand(hostHandCards);
            var guestHand = new Hand(guestHandCards);

            var hostPlayer = new Player(hostDeck, hostHand);
            var guestPlayer = new Player(guestDeck, guestHand);

            var lanes = LanesService.CreateEmptyLanes();

            Game game = new(hostConnectionId, guestConnectionId, gameCode, hostPlayer, guestPlayer, lanes);

            return game;
        }

        public bool MakeMove(Game game, Move move, bool playerIsHost)
        {
            var targetLaneIndex = move.PlaceCardAttempts[0].TargetLaneIndex;
            var lane = game.Lanes[targetLaneIndex];
            var multipleCardsPlayed = move.PlaceCardAttempts.Count > 1;

            if (!multipleCardsPlayed)
            {
                game.IsHostPlayersTurn = !game.IsHostPlayersTurn;
            }

            foreach (var placeCardAttempt in move.PlaceCardAttempts)
            {
                PlaceCardAndApplyGameRules(game, placeCardAttempt, lane, playerIsHost);
            }

            RemoveCardsFromHand(game, playerIsHost, move);

            var placedMultipleCards = move.PlaceCardAttempts.Count > 1;

            if (placedMultipleCards)
            {
                DrawCardsFromDeck(game, playerIsHost, 1);
            }
            else 
            {
                DrawCardsUntilHandAtFive(game, playerIsHost);
            }

            return true;
        }

        public void PassMove(Game game, bool playerIsHost)
        {
            var hostAndHostTurn = playerIsHost && game.IsHostPlayersTurn;
            var guestAndGuestTurn = !playerIsHost && !game.IsHostPlayersTurn;
            var isPlayersTurn = hostAndHostTurn || guestAndGuestTurn;

            if (!isPlayersTurn)
            {
                return;
            }

            DrawCardsUntilHandAtFive(game, playerIsHost);

            game.IsHostPlayersTurn = !game.IsHostPlayersTurn;
        }

        public void RearrangeHand(Game game, bool playerIsHost, List<Card> cards)
        {
            var existingCards = playerIsHost ? game.HostPlayer.Hand.Cards : game.GuestPlayer.Hand.Cards;
            bool newHandHasSameCards = existingCards.Except(cards).Any() && cards.Except(existingCards).Any();

            if (!newHandHasSameCards)
            {
                return;
            }

            if (playerIsHost)
            {
                game.HostPlayer.Hand.Cards = cards;
            }
            else
            {
                game.GuestPlayer.Hand.Cards = cards;
            }
        }

        private int RemoveCardsFromHand(Game game, bool playerIsHost, Move move)
        {
            var numCardsRemoved = 0;
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            
            foreach(var placeCardAttempt in move.PlaceCardAttempts)
            {
                CardService.RemoveCardWithMatchingKindAndSuit(player.Hand.Cards, placeCardAttempt.Card);
                numCardsRemoved++;
            }

            return numCardsRemoved;
        }

        private void DrawCardsFromDeck(Game game, bool playerIsHost, int numCardsToDraw)
        {
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;

            for(int i = 0; i < numCardsToDraw; i++)
            {
                var playersDeckHasCards = player.Deck.Cards.Any();

                if (!playersDeckHasCards)
                {
                    return;
                }

                var cardFromDeck = CardService.DrawCard(player.Deck);

                if (cardFromDeck is not null)
                {
                    player.Hand.AddCard(cardFromDeck);
                }
            }
        }

        private void DrawCardsUntilHandAtFive(Game game, bool playerIsHost)
        {
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;

            var numCardsInPlayersHand = player.Hand.Cards.Count;

            DrawCardsFromDeck(game, playerIsHost, 5 - numCardsInPlayersHand);
        }

        private void PlaceCardAndApplyGameRules(Game game, PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsHost)
        {
            var aceRuleTriggered = TriggerAceRuleIfAppropriate(lane, placeCardAttempt, playerIsHost);

            if (aceRuleTriggered)
            {
                return;
            }

            PlaceCard(lane, placeCardAttempt, playerIsHost);

            var middleCaptured = CaptureMiddleIfAppropriate(game, placeCardAttempt, playerIsHost);

            if (middleCaptured)
            {
                return;
            }

            WinLaneAndOrGameIfAppropriate(game, placeCardAttempt, playerIsHost);
        }

        private static void PlaceCard(Lane lane, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var targetRow = lane.Rows[placeCardAttempt.TargetRowIndex];
            placeCardAttempt.Card.PlayedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            targetRow.Add(placeCardAttempt.Card);
            lane.LastCardPlayed = placeCardAttempt.Card;
        }

        private bool CaptureMiddleIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var cardIsLastOnPlayerSide = playerIsHost ?
                placeCardAttempt.TargetRowIndex == 2 :
                placeCardAttempt.TargetRowIndex == 4;

            if (!cardIsLastOnPlayerSide)
            {
                return false;
            }

            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];

            var noAdvantage = lane.LaneAdvantage == PlayerOrNone.None;
            if (noAdvantage)
            {
                CaptureNoAdvantageLane(lane, placeCardAttempt, playerIsHost);
                return true;
            }

            var opponentAdvantage = playerIsHost ?
                lane.LaneAdvantage == PlayerOrNone.Guest :
                lane.LaneAdvantage == PlayerOrNone.Host;
            if (opponentAdvantage)
            {
                CaptureOpponentAdvantageLane(game, placeCardAttempt, playerIsHost);
                return true;
            }

            return false;
        }

        private void CaptureNoAdvantageLane(Lane lane, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var cardsFromLane = LanesService.GrabAllCardsAndClearLane(lane);

            // Put last placed card at top of pile
            cardsFromLane.Remove(placeCardAttempt.Card);
            cardsFromLane.Add(placeCardAttempt.Card);

            var middleRow = lane.Rows[3];
            middleRow.AddRange(cardsFromLane);
            lane.LaneAdvantage = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
        }

        private void CaptureOpponentAdvantageLane(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
            List<Card> topCardsOfFirstThreeRows = GetTopCardsOfFirstThreeRows(lane, playerIsHost);

            var remainingCardsInLane = LanesService.GrabAllCardsAndClearLane(lane);

            var middleRow = lane.Rows[3];
            middleRow.AddRange(topCardsOfFirstThreeRows);

            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            player.Deck.Cards.AddRange(remainingCardsInLane);
            CardService.ShuffleDeck(player.Deck);

            lane.LaneAdvantage = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
        }

        private static List<Card> GetTopCardsOfFirstThreeRows(Lane lane, bool playerIsHost)
        {
            List<Card> topCardsOfFirstThreeRows = new();

            if (playerIsHost)
            {
                for (int i = 0; i < 3; i++)
                {
                    var card = lane.Rows[i].Last();

                    if (card is not null)
                    {
                        topCardsOfFirstThreeRows.Add(card);
                    }
                }
            }
            else
            {
                for (int i = 6; i > 3; i--)
                {
                    var card = lane.Rows[i].Last();

                    if (card is not null)
                    {
                        topCardsOfFirstThreeRows.Add(card);
                    }
                }
            }

            return topCardsOfFirstThreeRows;
        }

        private bool TriggerAceRuleIfAppropriate(Lane lane, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var playerPlayedAnAce = placeCardAttempt.Card.Kind == Kind.Ace;
            if (!playerPlayedAnAce)
            {
                return false;
            }

            var opponentAceOnTopOfAnyRow = OpponentAceOnTopOfAnyRow(lane, playerIsHost);
            if (!opponentAceOnTopOfAnyRow)
            {
                return false;
            }

            _ = LanesService.GrabAllCardsAndClearLane(lane);
            lane.LastCardPlayed = null;
            lane.LaneAdvantage = PlayerOrNone.None;

            return true;
        }

        private bool WinLaneAndOrGameIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var placeCardInLastRow = playerIsHost ?
                placeCardAttempt.TargetRowIndex == 6 :
                placeCardAttempt.TargetRowIndex == 0;

            if (!placeCardInLastRow)
            {
                return false;
            }

            game.Lanes[placeCardAttempt.TargetLaneIndex].WonBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;

            var gameWon = WinGameIfAppropriate(game);

            if (gameWon)
            {
                return true;
            }

            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
            var allCardsInLane = LanesService.GrabAllCardsAndClearLane(lane);
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            player.Deck.Cards.AddRange(allCardsInLane);
            CardService.ShuffleDeck(player.Deck);


            if (game.RedJokerLaneIndex is null)
            {
                game.RedJokerLaneIndex = placeCardAttempt.TargetLaneIndex;
            }
            else
            {
                game.BlackJokerLaneIndex = placeCardAttempt.TargetLaneIndex;
            }

            return true;
        }

        private static bool WinGameIfAppropriate(Game game)
        {
            var lanesWonByHost = game.Lanes.Where(lane => lane.WonBy == PlayerOrNone.Host);
            var hostWon = lanesWonByHost.Count() == 2;
            if (hostWon)
            {
                game.WonBy = PlayerOrNone.Host;
                game.isRunning = false;
                return true;
            }

            var lanesWonByGuest = game.Lanes.Where(lane => lane.WonBy == PlayerOrNone.Guest);
            var guestWon = lanesWonByGuest.Count() == 2;
            if (guestWon)
            {
                game.WonBy = PlayerOrNone.Guest;
                game.isRunning = false;
                return true;
            }

            return false;
        }

        private bool OpponentAceOnTopOfAnyRow(Lane algoLane, bool playerIsHost)
        {
            foreach (var row in algoLane.Rows)
            {
                if (row.Count <= 0)
                {
                    continue;
                }

                var topCard = row.Last();

                var topCardIsAce = topCard.Kind == Kind.Ace;
                var topCardPlayedByOpponent = playerIsHost ?
                    topCard.PlayedBy == PlayerOrNone.Guest :
                    topCard.PlayedBy == PlayerOrNone.Host;
                if (topCardIsAce && topCardPlayedByOpponent)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
