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

        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode, DurationOption durationOption)
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

            var gameCreatedTimestampUTC = DateTime.UtcNow;

            Game game = new(
                hostConnectionId, 
                guestConnectionId, 
                gameCode, 
                hostPlayer, 
                guestPlayer, 
                lanes, 
                gameCreatedTimestampUTC, 
                durationOption);

            return game;
        }

        public bool MakeMove(Game game, Move move, bool playerIsHost)
        {
            var placedMultipleCards = move.PlaceCardAttempts.Count > 1;
            if (!placedMultipleCards)
            {
                game.IsHostPlayersTurn = !game.IsHostPlayersTurn;
            }

            var cardMovements = PlaceCardsAndApplyGameRules(game, move.PlaceCardAttempts, playerIsHost);
            var drawnCardMovements = placedMultipleCards
                ? DrawCardsFromDeck(game, playerIsHost, 1)
                : DrawCardsUntil(game, playerIsHost, 5);
            cardMovements.AddRange(drawnCardMovements);

            var playedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            var timeStampUTC = DateTime.UtcNow;
            var moveMade = new MoveMade(playedBy, move, timeStampUTC, cardMovements);
            game.MovesMade.Add(moveMade);

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
            
            var cardMovements = DrawCardsUntil(game, playerIsHost, 5);
            var move = new Move(new List<PlaceCardAttempt>());
            var playedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            var timeStampUTC = DateTime.UtcNow;

            game.MovesMade.Add(new MoveMade(playedBy, move, timeStampUTC, cardMovements));
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

        private List<List<CardMovement>> PlaceCardsAndApplyGameRules(Game game, List<PlaceCardAttempt> placeCardAttempts, bool playerIsHost)
        {
            return placeCardAttempts
                .SelectMany(placeCardAttempt => PlaceCardAndApplyGameRules(game, placeCardAttempt, playerIsHost))
                .ToList();
        }

        private List<List<CardMovement>> DrawCardsFromDeck(Game game, bool playerIsHost, int numCardsToDraw)
        {
            var cardMovements = new List<List<CardMovement>>();
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;

            for(int i = 0; i < numCardsToDraw; i++)
            {
                var cardFromDeck = CardService.DrawCard(player.Deck);

                if (cardFromDeck is null)
                {
                    return cardMovements;
                }

                var index = player.Hand.Cards.Count;

                var from = new CardStore()
                {
                    HostDeck = playerIsHost,
                    GuestDeck = !playerIsHost
                };

                var to = new CardStore()
                {
                    HostHandCardIndex = playerIsHost ? index : null,
                    GuestHandCardIndex = playerIsHost ? null : index
                };

                var cardMovement = new CardMovement(from, to, cardFromDeck);
                var cardMovementList = new List<CardMovement>() { cardMovement };
                cardMovements.Add(cardMovementList);

                player.Hand.AddCard(cardFromDeck);
            }

            return cardMovements;
        }

        private List<List<CardMovement>> DrawCardsUntil(Game game, bool playerIsHost, int maxNumCards)
        {
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            var numCardsInPlayersHand = player.Hand.Cards.Count;
            var numCardsNeeded = maxNumCards - numCardsInPlayersHand;

            return numCardsNeeded > 0
                ? DrawCardsFromDeck(game, playerIsHost, numCardsNeeded)
                : new List<List<CardMovement>>();
        }

        private List<List<CardMovement>> PlaceCardAndApplyGameRules(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var initialCardMovements = new List<CardMovement> { PlaceCard(game, placeCardAttempt, playerIsHost) };
            var cardMovements = new List<List<CardMovement>> { initialCardMovements };

            var aceRuleCardMovements = TriggerAceRuleIfAppropriate(game, placeCardAttempt, playerIsHost);
            if (aceRuleCardMovements.Any())
            {
                cardMovements.Add(aceRuleCardMovements);
                return cardMovements;
            }
            
            var capturedMiddleCardMovements = CaptureMiddleIfAppropriate(game, placeCardAttempt, playerIsHost);
            if (capturedMiddleCardMovements.Any())
            {
                cardMovements.Add(capturedMiddleCardMovements);
                return cardMovements;
            }

            var laneWonCardMovements = WinLaneAndOrGameIfAppropriate(game, placeCardAttempt, playerIsHost);
            if (laneWonCardMovements.Any())
            {
                cardMovements.Add(laneWonCardMovements);
            }

            return cardMovements;
        }

        private CardMovement PlaceCard(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
            var currentPlayedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            var targetRow = lane.Rows[placeCardAttempt.TargetRowIndex];
            var cardReinforced = targetRow.Count > 0 && targetRow.Last().PlayedBy == currentPlayedBy;

            // This could cause issues with not being able to play the same kind of card as a reinforcement.
            if (!cardReinforced)
            {
                lane.LastCardPlayed = placeCardAttempt.Card;
            }

            placeCardAttempt.Card.PlayedBy = currentPlayedBy;
            targetRow.Add(placeCardAttempt.Card);

            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            var indexInHand = CardService.RemoveCardWithMatchingKindAndSuit(player.Hand.Cards, placeCardAttempt.Card);

            if (indexInHand is null)
            {
                throw new Exception("Attempted to place a card that a player did not have.");
            }

            var from = new CardStore
            {
                HostHandCardIndex = playerIsHost ? indexInHand : null,
                GuestHandCardIndex = playerIsHost ? null : indexInHand
            };

            var to = new CardStore
            {
                CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, placeCardAttempt.TargetRowIndex)
            };

            return new CardMovement(from, to, placeCardAttempt.Card);
        }

        private List<CardMovement> CaptureMiddleIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var cardIsLastOnPlayerSide = playerIsHost ?
                placeCardAttempt.TargetRowIndex == 2 :
                placeCardAttempt.TargetRowIndex == 4;

            if (!cardIsLastOnPlayerSide)
            {
                return new List<CardMovement>();
            }

            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];

            var noAdvantage = lane.LaneAdvantage == PlayerOrNone.None;
            if (noAdvantage)
            {
                return CaptureNoAdvantageLane(lane, placeCardAttempt, playerIsHost);
            }

            var opponentAdvantage = playerIsHost ?
                lane.LaneAdvantage == PlayerOrNone.Guest :
                lane.LaneAdvantage == PlayerOrNone.Host;
            if (opponentAdvantage)
            {
                return CaptureOpponentAdvantageLane(game, placeCardAttempt, playerIsHost);
            }

            return new List<CardMovement>();
        }

        private List<CardMovement> CaptureNoAdvantageLane(Lane lane, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var advantagePlayer = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            var laneCardsAndRowIndexes = LanesService.GrabAllCardsFromLane(lane)
                .OrderBy(cardAndRowIndex => cardAndRowIndex.Item1.PlayedBy == advantagePlayer)
                .ThenBy(cardAndRowIndex => playerIsHost ? cardAndRowIndex.Item2 : -cardAndRowIndex.Item2)
                .ToList();

            var laneCards = laneCardsAndRowIndexes.Select(cardAndRowIndex => cardAndRowIndex.Item1);
            var middleRow = lane.Rows[3];
            middleRow.AddRange(laneCards);
            lane.LaneAdvantage = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;

            return GetCardMovementsFromCapturedMiddleCards(laneCardsAndRowIndexes, placeCardAttempt);
        }

        private List<CardMovement> GetCardMovementsFromCapturedMiddleCards(List<(Card, int)> cardsAndRowIndexes, PlaceCardAttempt placeCardAttempt)
        {
            var cardMovements = new List<CardMovement>();

            foreach (var (card, rowIndex) in cardsAndRowIndexes)
            {
                var to = new CardStore
                {
                    CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, 3)
                };

                var from = new CardStore
                {
                    CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, rowIndex)
                };

                cardMovements.Add(new CardMovement(from, to, card));
            }

            return cardMovements;
        }

        private List<CardMovement> CaptureOpponentAdvantageLane(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
            var topCardsWithRowIndexes = GetTopCardsOfFirstThreeRows(lane, playerIsHost);
            var topCards = topCardsWithRowIndexes
                .Select(cardsWithRowIndexes => cardsWithRowIndexes.Item1)
                .ToList();
            var remainingCardsInLaneWithRowIndexes = LanesService.GrabAllCardsFromLane(lane);
            var remainingCardsInLane = remainingCardsInLaneWithRowIndexes
                .Select(x => x.Item1)
                .Where(card => !topCards.Any(topCard => topCard.Suit == card.Suit && topCard.Kind == card.Kind))
                .ToList();
            var middleRow = lane.Rows[3];
            middleRow.AddRange(topCards);

            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            player.Deck.Cards.AddRange(remainingCardsInLane);

            var cardMovements = remainingCardsInLane.Select(card =>
            {
                var from = new CardStore()
                {
                    CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, 3)
                };

                var to = new CardStore()
                {
                    HostDeck = true
                };

                return new CardMovement(from, to, card);
            }).ToList();

            CardService.ShuffleDeck(player.Deck);
            lane.LaneAdvantage = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;

            cardMovements.AddRange(GetCardMovementsFromCapturedMiddleCards(topCardsWithRowIndexes, placeCardAttempt));
            cardMovements.AddRange(GetCardMovementsFromCapturingCards(remainingCardsInLaneWithRowIndexes, placeCardAttempt, playerIsHost));
            
            return cardMovements;
        }

        private List<CardMovement> GetCardMovementsFromCapturingCards(List<(Card, int)> cardsWithRowIndexes, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var cardMovements = new List<CardMovement>();

            foreach (var (card, rowIndex) in cardsWithRowIndexes)
            {
                var from = new CardStore()
                {
                    CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, rowIndex)
                };

                var to = new CardStore()
                {
                    HostDeck = playerIsHost,
                    GuestDeck = !playerIsHost
                };

                cardMovements.Add(new CardMovement(from, to, card));
            }

            return cardMovements;
        }

        /// <returns>Cards alongside their row indexes.</returns>
        private static List<(Card, int)> GetTopCardsOfFirstThreeRows(Lane lane, bool playerIsHost)
        {
            List<(Card, int)> topCardsOfFirstThreeRows = new();

            if (playerIsHost)
            {
                for (int i = 0; i < 3; i++)
                {
                    var card = lane.Rows[i].Last();

                    if (card is not null)
                    {
                        topCardsOfFirstThreeRows.Add((card, i));
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
                        topCardsOfFirstThreeRows.Add((card, i));
                    }
                }
            }

            return topCardsOfFirstThreeRows;
        }

        private List<CardMovement> TriggerAceRuleIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var playerPlayedAnAce = placeCardAttempt.Card.Kind == Kind.Ace;
            if (!playerPlayedAnAce)
            {
                return new List<CardMovement>();
            }

            var laneIndex = placeCardAttempt.TargetLaneIndex;
            var lane = game.Lanes[laneIndex];
            var opponentAceOnTopOfAnyRow = OpponentAceOnTopOfAnyRow(lane, playerIsHost);
            if (!opponentAceOnTopOfAnyRow)
            {
                return new List<CardMovement>();
            }

            lane.LastCardPlayed = null;
            lane.LaneAdvantage = PlayerOrNone.None;
            var destroyedCardsAndRowIndexes = LanesService.GrabAllCardsFromLane(lane);

            return GetCardMovementsFromDestroyedCards(destroyedCardsAndRowIndexes, laneIndex);
        }

        private static List<CardMovement> GetCardMovementsFromDestroyedCards(List<(Card, int)> destroyedCardsAndRowIndexes, int laneIndex)
        {
            var cardMovements = new List<CardMovement>();

            foreach (var (destroyedCard, rowIndex) in destroyedCardsAndRowIndexes)
            {
                var from = new CardStore
                {
                    CardPosition = new CardPosition(laneIndex, rowIndex)
                };

                var to = new CardStore
                {
                    Destroyed = true
                };

                cardMovements.Add(new CardMovement(from, to, destroyedCard));
            }

            return cardMovements;
        }

        private List<CardMovement> WinLaneAndOrGameIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var placeCardInLastRow = playerIsHost ?
                placeCardAttempt.TargetRowIndex == 6 :
                placeCardAttempt.TargetRowIndex == 0;

            if (!placeCardInLastRow)
            {
                return new List<CardMovement>();
            }

            game.Lanes[placeCardAttempt.TargetLaneIndex].WonBy = playerIsHost 
                ? PlayerOrNone.Host 
                : PlayerOrNone.Guest;

            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
            var allCardsInLaneWithRowIndexes = LanesService.GrabAllCardsFromLane(lane);
            var allCardsInLane = allCardsInLaneWithRowIndexes
                .Select(cardWithRowIndex => cardWithRowIndex.Item1)
                .ToList();

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

            WinGameIfAppropriate(game);

            return GetCardMovementsFromWonCards(allCardsInLaneWithRowIndexes, placeCardAttempt, playerIsHost);
        }

        private List<CardMovement> GetCardMovementsFromWonCards(List<(Card, int)> cardsWithRowIndexes, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var cardMovements = new List<CardMovement>();

            foreach(var (card, rowIndex) in cardsWithRowIndexes)
            {
                var from = new CardStore()
                {
                    CardPosition = new CardPosition(placeCardAttempt.TargetLaneIndex, rowIndex)
                };

                var to = new CardStore()
                {
                    HostDeck = playerIsHost,
                    GuestDeck = !playerIsHost
                };

                cardMovements.Add(new CardMovement(from, to, card));
            }

            return cardMovements;
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

        private static bool OpponentAceOnTopOfAnyRow(Lane lane, bool playerIsHost)
        {
            foreach (var row in lane.Rows)
            {
                if (OpponentAceOnTopOfRow(row, playerIsHost))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool OpponentAceOnTopOfRow(List<Card> row, bool playerIsHost)
        {
            if (row.Count <= 0)
            {
                return false;
            }

            var topCard = row.Last();
            var topCardIsAce = topCard.Kind == Kind.Ace;
            var topCardPlayedByOpponent = playerIsHost ?
                topCard.PlayedBy == PlayerOrNone.Guest :
                topCard.PlayedBy == PlayerOrNone.Host;

            return topCardIsAce && topCardPlayedByOpponent;
        }
    }
}
