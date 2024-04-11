﻿using LanesBackend.Interfaces;
using LanesBackend.Models;
using LanesBackend.Exceptions;
using LanesBackend.Util;

namespace LanesBackend.Logic
{
    public class GameService : IGameService
    {
        private readonly ILanesService LanesService;

        private readonly ICardService CardService;

        private readonly IGameCache GameCache;

        public GameService(
            ILanesService lanesService,
            ICardService cardService,
            IGameCache gameCache)
        {
            LanesService = lanesService;
            CardService = cardService;
            GameCache = gameCache;
        }

        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode, DurationOption durationOption, bool playerIsHost)
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

            var candidateMoves = GetCandidateMoves(game, game.IsHostPlayersTurn);
            game.CandidateMoves.Add(candidateMoves);

            GameCache.AddGame(game);

            return game;
        }

        public Game MakeMove(string connectionId, Move move, List<Card>? rearrangedCardsInHand)
        {
            var game = GameCache.FindGameByConnectionId(connectionId);
            if (game is null)
            {
                throw new GameNotExistsException();
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            var placedMultipleCards = move.PlaceCardAttempts.Count > 1;
            var cardMovements = PlaceCardsAndApplyGameRules(game, move.PlaceCardAttempts, playerIsHost);

            if (rearrangedCardsInHand is not null)
            {
                RearrangeHand(connectionId, rearrangedCardsInHand);
            }

            var drawnCardMovements = placedMultipleCards
                ? DrawCardsFromDeck(game, playerIsHost, 1)
                : DrawCardsUntil(game, playerIsHost, 5);
            cardMovements.AddRange(drawnCardMovements);

            var playedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            var moveMade = new MoveMade(playedBy, move, DateTime.UtcNow, cardMovements);
            game.MovesMade.Add(moveMade);

            if (!placedMultipleCards)
            {
                game.IsHostPlayersTurn = !game.IsHostPlayersTurn;
            }

            var candidateMoves = GetCandidateMoves(game, game.IsHostPlayersTurn);
            game.CandidateMoves.Add(candidateMoves);
            var bothPlayersHaveMoves = BothPlayersHaveMoves(candidateMoves, game);
            if (!bothPlayersHaveMoves)
            {
                game.HasEnded = true;
            }

            if (game.HasEnded)
            {
                game.GameEndedTimestampUTC = DateTime.UtcNow;
                GameCache.RemoveGameByConnectionId(connectionId);
            }

            return game;
        }

        public Game PassMove(string connectionId)
        {
            var game = GameCache.FindGameByConnectionId(connectionId);
            if (game is null)
            {
                throw new GameNotExistsException();
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            var hostAndHostTurn = playerIsHost && game.IsHostPlayersTurn;
            var guestAndGuestTurn = !playerIsHost && !game.IsHostPlayersTurn;
            var isPlayersTurn = hostAndHostTurn || guestAndGuestTurn;
            if (!isPlayersTurn)
            {
                throw new NotPlayersTurnException();
            }
            
            var cardMovements = DrawCardsUntil(game, playerIsHost, 5);
            var move = new Move(new List<PlaceCardAttempt>());
            var playedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            var timeStampUTC = DateTime.UtcNow;
            game.MovesMade.Add(new MoveMade(playedBy, move, timeStampUTC, cardMovements));
            game.IsHostPlayersTurn = !game.IsHostPlayersTurn;

            var candidateMoves = GetCandidateMoves(game, game.IsHostPlayersTurn);
            game.CandidateMoves.Add(candidateMoves);

            return game;
        }

        public Hand RearrangeHand(string connectionId, List<Card> cards)
        {
            var game = GameCache.FindGameByConnectionId(connectionId);
            if (game is null)
            {
                throw new GameNotExistsException();
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            var existingHand = playerIsHost ? game.HostPlayer.Hand : game.GuestPlayer.Hand;
            var existingCards = existingHand.Cards;
            bool containsDifferentCards = ContainsDifferentCards(existingCards, cards);
            if (containsDifferentCards)
            {
                throw new ContainsDifferentCardsException();
            }

            existingHand.Cards = cards;
            return existingHand;
        }

        public Game? RemoveGame(string connectionId)
        {
            var game = GameCache.RemoveGameByConnectionId(connectionId);
            if (game is not null)
            {
                game.HasEnded = true;
                game.GameEndedTimestampUTC = DateTime.UtcNow;
            }

            return game;
        }

        public Game? FindGame(string connectionId)
        {
            return GameCache.FindGameByConnectionId(connectionId);
        }

        public Game AcceptDrawOffer(string connectionId)
        {
            var game = GameCache.FindGameByConnectionId(connectionId);
            if (game is null)
            {
                throw new GameNotExistsException();
            }

            // [Security Hardening]: Actually verify that the opponent offererd a draw to begin with.

            game.HasEnded = true;
            game.GameEndedTimestampUTC = DateTime.UtcNow;
            GameCache.RemoveGameByConnectionId(connectionId);

            return game;
        }

        public Game ResignGame(string connectionId)
        {
            var game = GameCache.FindGameByConnectionId(connectionId);
            if (game is null)
            {
                throw new GameNotExistsException();
            }

            var playerIsHost = game.HostConnectionId == connectionId;
            game.WonBy = playerIsHost ? PlayerOrNone.Guest : PlayerOrNone.Host;

            game.HasEnded = true;
            game.GameEndedTimestampUTC = DateTime.UtcNow;
            GameCache.RemoveGameByConnectionId(connectionId);

            return game;
        }

        public Game EndGame(string connectionId)
        {
            var game = GameCache.FindGameByConnectionId(connectionId);
            if (game is null)
            {
                throw new GameNotExistsException();
            }

            game.HasEnded = true;
            game.GameEndedTimestampUTC = DateTime.UtcNow;
            GameCache.RemoveGameByConnectionId(connectionId);

            return game;
        }

        private List<List<CardMovement>> PlaceCardsAndApplyGameRules(Game game, List<PlaceCardAttempt> placeCardAttempts, bool playerIsHost)
        {
            return placeCardAttempts
                .SelectMany(placeCardAttempt => PlaceCardAndApplyGameRules(game, placeCardAttempt, playerIsHost))
                .ToList();
        }

        private static bool BothPlayersHaveMoves(List<CandidateMove> playerCandidateMoves, Game game)
        {
            var allCandidateMovesInvalid = playerCandidateMoves.All(move => !move.IsValid);
            if (!playerCandidateMoves.Any() || allCandidateMovesInvalid)
            {
                var opponentCandidateMoves = GetCandidateMoves(game, !game.IsHostPlayersTurn);
                var allOpponentCandidateMovesInvalid = opponentCandidateMoves.All(move => !move.IsValid);
                if (!opponentCandidateMoves.Any() || allOpponentCandidateMovesInvalid)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsDifferentCards(List<Card> list1, List<Card> list2)
        {
            if (list1.Count != list2.Count)
            {
                return true;
            }

            for (var i = 0; i < list1.Count; i++)
            {
                var card1 = list1[i];
                var hasCard = false;
                
                for (var j = 0; j < list2.Count; j++)
                {
                    var card2 = list2[j];

                    var kindMatches = card1.Kind == card2.Kind;
                    var suitMatches = card1.Suit == card2.Suit;
                    
                    if (kindMatches && suitMatches)
                    {
                        hasCard = true;
                        break;
                    }
                }

                if (!hasCard)
                {
                    return true;
                }
            }
            return false;
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
                cardMovements.AddRange(capturedMiddleCardMovements);
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

        private List<List<CardMovement>> CaptureMiddleIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var cardIsLastOnPlayerSide = playerIsHost ?
                placeCardAttempt.TargetRowIndex == 2 :
                placeCardAttempt.TargetRowIndex == 4;

            if (!cardIsLastOnPlayerSide)
            {
                return new List<List<CardMovement>>();
            }

            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];

            var noAdvantage = lane.LaneAdvantage == PlayerOrNone.None;
            if (noAdvantage)
            {
                return new List<List<CardMovement>>
                {
                    CaptureNoAdvantageLane(lane, placeCardAttempt, playerIsHost)
                };
            }

            var opponentAdvantage = playerIsHost ?
                lane.LaneAdvantage == PlayerOrNone.Guest :
                lane.LaneAdvantage == PlayerOrNone.Host;
            if (opponentAdvantage)
            {
                return CaptureOpponentAdvantageLane(game, placeCardAttempt, playerIsHost);
            }

            return new List<List<CardMovement>>();
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

            return GetCardMovementsGoingToTheMiddle(laneCardsAndRowIndexes, placeCardAttempt);
        }

        private List<CardMovement> GetCardMovementsGoingToTheMiddle(List<(Card, int)> cardsAndRowIndexes, PlaceCardAttempt placeCardAttempt)
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

        private List<List<CardMovement>> CaptureOpponentAdvantageLane(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];
            var topCardsWithRowIndexes = GrabTopCardsOfFirstThreeRows(lane, playerIsHost);
            var topCards = topCardsWithRowIndexes
                .Select(cardsWithRowIndexes => cardsWithRowIndexes.Item1)
                .ToList();
            var remainingCardsInLaneWithRowIndexes = LanesService.GrabAllCardsFromLane(lane);
            var remainingCardsInLane = remainingCardsInLaneWithRowIndexes
                .Select(x => x.Item1)
                .ToList();

            var middleRow = lane.Rows[3];
            middleRow.AddRange(topCards);

            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            CardService.ShuffleDeck(player.Deck);
            lane.LaneAdvantage = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;

            var cardMovements = GetCardMovementsGoingToTheMiddle(topCardsWithRowIndexes, placeCardAttempt);
            cardMovements.AddRange(GetCapturedCardMovementsGoingToTheDeck(remainingCardsInLaneWithRowIndexes, placeCardAttempt, playerIsHost));

            return new List<List<CardMovement>>
            {
                cardMovements
            };
        }

        private List<CardMovement> GetCapturedCardMovementsGoingToTheDeck(List<(Card, int)> cardsWithRowIndexes, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
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
        private static List<(Card, int)> GrabTopCardsOfFirstThreeRows(Lane lane, bool playerIsHost)
        {
            List<(Card, int)> topCardsOfFirstThreeRows = new();

            int startRow = playerIsHost ? 0 : 6;
            int endRow = playerIsHost ? 3 : 4;
            int step = playerIsHost ? 1 : -1;

            for (int i = startRow; playerIsHost ? i < endRow : i >= endRow; i += step)
            {
                var row = lane.Rows[i];

                if (row.Count > 0)
                {
                    var card = row.Last();
                    row.RemoveAt(row.Count - 1);

                    topCardsOfFirstThreeRows.Add((card, i));
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
                game.HasEnded = true;
                return true;
            }

            var lanesWonByGuest = game.Lanes.Where(lane => lane.WonBy == PlayerOrNone.Guest);
            var guestWon = lanesWonByGuest.Count() == 2;
            if (guestWon)
            {
                game.WonBy = PlayerOrNone.Guest;
                game.HasEnded = true;
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

        private static List<CandidateMove> GetCandidateMoves(Game game, bool forHostPlayer)
        {
            var player = forHostPlayer ? game.HostPlayer : game.GuestPlayer;
            var candidateMoves = new List<CandidateMove>();

            foreach(var cardInHand in player.Hand.Cards)
            {
                var cardCandidateMoves = GetCandidateMoves(game, cardInHand, forHostPlayer);
                candidateMoves.AddRange(cardCandidateMoves);
            }

            return candidateMoves;
        }

        private static List<CandidateMove> GetCandidateMoves(Game game, Card card, bool forHostPlayer)
        {
            var candidateMoves = new List<CandidateMove>();

            for (var rowIndex = 0; rowIndex < 7; rowIndex++)
            {
                for (var laneIndex = 0; laneIndex < 5; laneIndex++)
                {
                    var placeCardAttempt = new PlaceCardAttempt(card, laneIndex, rowIndex);
                    var placeCardAttempts = new List<PlaceCardAttempt> { placeCardAttempt };
                    var move = new Move(placeCardAttempts);
                    var candidateMove = GetCandidateMove(move, game, forHostPlayer);
                    candidateMoves.Add(candidateMove);

                    if (GameUtil.IsDefensive(placeCardAttempt, game.IsHostPlayersTurn))
                    {
                        var player = forHostPlayer ? game.HostPlayer : game.GuestPlayer;
                        var cardsInHand = player.Hand.Cards;
                        var placeMultipleCandidateMoves = GetPlaceMultipleCandidateMoves(placeCardAttempt, cardsInHand, game, forHostPlayer);
                        candidateMoves.AddRange(placeMultipleCandidateMoves);
                    }
                }
            }

            return candidateMoves;
        }

        private static CandidateMove GetCandidateMove(Move move, Game game,bool forHostPlayer)
        {
            var invalidReason = GameUtil.GetReasonIfMoveInvalid(move, game, forHostPlayer);
            var isValid = invalidReason is null;
            var candidateMove = new CandidateMove(move, isValid, invalidReason);
            return candidateMove;
        }

        private static List<CandidateMove> GetPlaceMultipleCandidateMoves(
            PlaceCardAttempt initialPlaceCardAttempt, 
            List<Card> cardsInHand,
            Game game,
            bool forHostPlayer)
        {
            var candidateMoves = new List<CandidateMove>();

            var candidateCardsInHand = cardsInHand.Where(cardInHand => 
                GameUtil.KindMatches(initialPlaceCardAttempt.Card, cardInHand) && 
                !GameUtil.SuitMatches(initialPlaceCardAttempt.Card, cardInHand)).ToList();

            List<List<Card>> candidateCardPermutationSubsets = PermutationsUtil.GetSubsetsPermutations(candidateCardsInHand);

            foreach (var candidateCards in candidateCardPermutationSubsets)
            {
                var totalCandidatePlaceCardAttempts = new List<PlaceCardAttempt> { initialPlaceCardAttempt };
                var candidatePlaceCardAttempts = new List<PlaceCardAttempt>();

                for (var i = 0; i < candidateCards.Count; i++)
                {
                    var rowIndex = forHostPlayer
                        ? initialPlaceCardAttempt.TargetRowIndex + 1 + i
                        : initialPlaceCardAttempt.TargetRowIndex - 1 - i;

                    var card = candidateCards[i];
                    if (card is not null)
                    {
                        var attempt = new PlaceCardAttempt(card, initialPlaceCardAttempt.TargetLaneIndex, rowIndex);
                        candidatePlaceCardAttempts.Add(attempt);
                    }
                }

                totalCandidatePlaceCardAttempts.AddRange(candidatePlaceCardAttempts);
                var move = new Move(totalCandidatePlaceCardAttempts);
                var candidateMove = GetCandidateMove(move, game, forHostPlayer);
                candidateMoves.Add(candidateMove);
            }

            return candidateMoves;
        }
    }
}
