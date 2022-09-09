using LanesBackend.Interfaces;
using LanesBackend.Interfaces.GameEngine;
using LanesBackend.Models;
using LanesBackend.Models.GameEngine;

namespace LanesBackend.Logic
{
    public class GameEngineService : IGameEngineService
    {
        private readonly IMoveChecksService MoveChecksService;

        private readonly IAlgoModelMapperService AlgoModelMapperService;

        private readonly IAlgoMoveChecksService AlgoMoveChecksService;

        private readonly IAlgoLanesService AlgoLanesService;

        private readonly IDeckService DeckService;

        public GameEngineService(
            IMoveChecksService moveChecksService,
            IAlgoModelMapperService algoModelMapperService,
            IAlgoMoveChecksService algoMoveChecksService,
            IAlgoLanesService algoLanesService,
            IDeckService deckService)
        {
            MoveChecksService = moveChecksService;
            AlgoModelMapperService = algoModelMapperService;
            AlgoMoveChecksService = algoMoveChecksService;
            AlgoLanesService = algoLanesService;
            DeckService = deckService;
        }

        public bool MoveIsValid(Game game, Move move, bool playerIsHost)
        {
            var isPlayersTurn = MoveChecksService.IsPlayersTurn(game, playerIsHost);
            if (!isPlayersTurn)
            {
                Console.WriteLine("Client broke the rules: Tried to make a move out of turn.");
                return false;
            }

            var moveHasPlaceCardAttempts = move.PlaceCardAttempts.Any();
            if (!moveHasPlaceCardAttempts)
            {
                Console.WriteLine("Client broke the rules: Tried to make a move with no place card attempts.");
                return false;
            }

            var isEntireMoveOnSameLane = MoveChecksService.IsEntireMoveOnSameLane(move);
            if (!isEntireMoveOnSameLane)
            {
                Console.WriteLine("Client broke the rules: Tried to place cards on multiple lanes in one move.");
                return false;
            }

            var isAnyPlaceCardAttemptInMiddle = MoveChecksService.IsAnyPlaceCardAttemptInMiddle(move);
            if (isAnyPlaceCardAttemptInMiddle)
            {
                Console.WriteLine("Client broke the rules: Tried to place a card in the middle row.");
                return false;
            }

            var placeCardAttempt = move.PlaceCardAttempts[0];
            var targetLaneIndex = placeCardAttempt.TargetLaneIndex;
            var lane = game.Lanes[targetLaneIndex];

            var laneWon = lane.WonBy != PlayerOrNone.None;
            if (laneWon)
            {
                Console.WriteLine("Client broke the rules: Tried to move a lane that has been won.");
                return false;
            }

            var algoLane = AlgoModelMapperService.ToAlgoLane(lane, playerIsHost);
            var algoMove = AlgoModelMapperService.ToAlgoMove(move, playerIsHost);

            // For now assume all moves are one place card attempt.
            var algoPlaceCardAttempt = algoMove.PlaceCardAttempts.First();
            var algoCard = algoPlaceCardAttempt.Card;

            var moveStartsOnPlayerSide = algoPlaceCardAttempt.TargetRowIndex < 3;
            var playerHasAdvantage = algoLane.LaneAdvantage == AlgoPlayer.Player;
            if (moveStartsOnPlayerSide && playerHasAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their own side when they have the advantage.");
                return false;
            }

            var moveStartsOnOpponentSide = algoMove.PlaceCardAttempts.First().TargetRowIndex > 3;
            var opponentHasAdvantage = algoLane.LaneAdvantage == AlgoPlayer.Opponent;
            if (moveStartsOnOpponentSide && opponentHasAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their opponent's side when they have their opponent has the advantage.");
                return false;
            }

            var noAdvantage = algoLane.LaneAdvantage == AlgoPlayer.None;
            if (moveStartsOnOpponentSide && noAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their opponent's side when there is no advantage.");
                return false;
            }

            var allPreviousRowsOccupied = AlgoMoveChecksService.AllPreviousRowsOccupied(algoLane, algoPlaceCardAttempt.TargetRowIndex);
            if (moveStartsOnPlayerSide && !allPreviousRowsOccupied)
            {
                Console.WriteLine("Client broke the rules: Tried to move on position where previous rows aren't occupied.");
                return false;
            }

            var cardIsAce = algoCard.Kind == Kind.Ace;
            var opponentAceOnTopOfAnyRow = AlgoMoveChecksService.OpponentAceOnTopOfAnyRow(algoLane);
            var playedAceToNukeRow = cardIsAce && opponentAceOnTopOfAnyRow;
            var cardsHaveMatchingSuitOrKind =
                lane.LastCardPlayed is not null &&
                (algoCard.Suit == lane.LastCardPlayed.Suit ||
                algoCard.Kind == lane.LastCardPlayed.Kind);
            if (lane.LastCardPlayed is not null && !cardsHaveMatchingSuitOrKind && !playedAceToNukeRow)
            {
                Console.WriteLine("Client broke the rules: Tried to play a card that has other suit or other kind than the last card played OR not an ace to nuke the row.");
                return false;
            }

            var targetRow = algoLane.Rows[placeCardAttempt.TargetRowIndex];
            var targetCard = targetRow.Any() ? targetRow.Last() : null;

            // Can't reinforce with different suit card.
            if (
              targetCard is not null &&
              targetCard.PlayedBy == AlgoPlayer.Player &&
              targetCard.Suit != algoCard.Suit
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a different suit.");
                return false;
            }

            // Can't reinforce a lesser card.
            if (
              targetCard is not null &&
              targetCard.PlayedBy == AlgoPlayer.Player &&
              !AlgoMoveChecksService.CardTrumpsCard(algoCard, targetCard)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a lesser card.");
                return false;
            }

            // Can't capture a lesser card.
            if (
              targetCard is not null &&
              targetCard.Suit == algoCard.Suit &&
              !AlgoMoveChecksService.CardTrumpsCard(algoCard, targetCard)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a lesser card.");
                return false;
            }

            return true;
        }

        public void MakeMove(Game game, Move move, bool playerIsHost)
        {
            var targetLaneIndex = move.PlaceCardAttempts[0].TargetLaneIndex;
            var lane = game.Lanes[targetLaneIndex];
            var algoLane = AlgoModelMapperService.ToAlgoLane(lane, playerIsHost);
            var algoMove = AlgoModelMapperService.ToAlgoMove(move, playerIsHost);

            var isPairMove = algoMove.PlaceCardAttempts.Count > 1;

            if (!isPairMove)
            {
                game.IsHostPlayersTurn = !game.IsHostPlayersTurn;
            }

            foreach (var placeCardAttempt in algoMove.PlaceCardAttempts)
            {
                PlaceCardAndApplyGameRules(game, placeCardAttempt, algoLane, playerIsHost);
            }

            var algoLaneBackToOriginal = AlgoModelMapperService.FromAlgoLane(algoLane, playerIsHost);
            game.Lanes[targetLaneIndex] = algoLaneBackToOriginal;
        }

        private void PlaceCardAndApplyGameRules(Game game, AlgoPlaceCardAttempt algoPlaceCardAttempt, AlgoLane algoLane, bool playerIsHost)
        {
            var aceRuleTriggered = TriggerAceRuleIfAppropriate(algoPlaceCardAttempt, algoLane);

            if (aceRuleTriggered)
            {
                return;
            }

            PlaceCard(algoLane, algoPlaceCardAttempt);

            var middleCaptured = CaptureMiddleIfAppropriate(game, algoPlaceCardAttempt, algoLane, playerIsHost);

            if (middleCaptured)
            {
                return;
            }

            _ = WinLaneIfAppropriate(game, algoPlaceCardAttempt, algoLane, playerIsHost);
        }

        private static void PlaceCard(AlgoLane algoLane, AlgoPlaceCardAttempt algoPlaceCardAttempt)
        {
            var targetRow = algoLane.Rows[algoPlaceCardAttempt.TargetRowIndex];
            algoPlaceCardAttempt.Card.PlayedBy = AlgoPlayer.Player;
            targetRow.Add(algoPlaceCardAttempt.Card);
            algoLane.LastCardPlayed = algoPlaceCardAttempt.Card;
        }

        private bool CaptureMiddleIfAppropriate(Game game, AlgoPlaceCardAttempt algoPlaceCardAttempt, AlgoLane algoLane, bool playerIsHost)
        {
            var cardIsLastOnHostSide = algoPlaceCardAttempt.TargetRowIndex == 2;

            if (!cardIsLastOnHostSide)
            {
                return false;
            }

            if (algoLane.LaneAdvantage == AlgoPlayer.None)
            {
                CaptureNoAdvantageLane(algoLane, algoPlaceCardAttempt);
            }
            else if (algoLane.LaneAdvantage == AlgoPlayer.Opponent)
            {
                CaptureOpponentAdvantageLane(game, algoLane, playerIsHost);
            }

            return true;
        }

        private void CaptureNoAdvantageLane(AlgoLane algoLane, AlgoPlaceCardAttempt algoPlaceCardAttempt)
        {
            var cardsFromLane = AlgoLanesService.GrabAllCardsAndClearLane(algoLane);

            // Put last placed card at top of pile
            cardsFromLane.Remove(algoPlaceCardAttempt.Card);
            cardsFromLane.Add(algoPlaceCardAttempt.Card);

            var middleRow = algoLane.Rows[3];
            middleRow.AddRange(cardsFromLane);
            algoLane.LaneAdvantage = AlgoPlayer.Player;
        }

        private void CaptureOpponentAdvantageLane(Game game, AlgoLane algoLane, bool playerIsHost)
        {
            List<AlgoCard> topCardsOfFirstThreeRows = new();

            for (int i = 0; i < 3; i++)
            {
                var algoCard = algoLane.Rows[i].Last();

                if (algoCard is not null)
                {
                    topCardsOfFirstThreeRows.Add(algoCard);
                }
            }

            var remainingAlgoCardsInLane = AlgoLanesService.GrabAllCardsAndClearLane(algoLane);

            var middleRow = algoLane.Rows[3];
            middleRow.AddRange(topCardsOfFirstThreeRows);

            // This is bad. We are in algorithm territory manipulating non-algorithmic models.
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            var remainingCardsInLane = remainingAlgoCardsInLane.Select(algoCard => AlgoModelMapperService.FromAlgoCard(algoCard, playerIsHost));
            player.Deck.Cards.AddRange(remainingCardsInLane);
            DeckService.ShuffleDeck(player.Deck);

            algoLane.LaneAdvantage = AlgoPlayer.Player;
        }

        private bool TriggerAceRuleIfAppropriate(AlgoPlaceCardAttempt algoPlaceCardAttempt, AlgoLane algoLane)
        {
            var playerPlayedAnAce = algoPlaceCardAttempt.Card.Kind == Kind.Ace;
            if (!playerPlayedAnAce)
            {
                return false;
            }

            var opponentAceOnTopOfAnyRow = AlgoMoveChecksService.OpponentAceOnTopOfAnyRow(algoLane);
            if (!opponentAceOnTopOfAnyRow)
            {
                return false;
            }

            _ = AlgoLanesService.GrabAllCardsAndClearLane(algoLane);

            return true;
        }

        private bool WinLaneIfAppropriate(Game game, AlgoPlaceCardAttempt algoPlaceCardAttempt, AlgoLane algoLane, bool playerIsHost)
        {
            var placeCardInLastRow = algoPlaceCardAttempt.TargetRowIndex == 6;

            if (!placeCardInLastRow)
            {
                return false;
            }

            algoLane.WonBy = AlgoPlayer.Player;
            // This is bad. We are in algorithm territory manipulating non-algorithmic models.
            var allAlgoCardsInLane = AlgoLanesService.GrabAllCardsAndClearLane(algoLane);
            var allCardsInLane = allAlgoCardsInLane.Select(algoCard => AlgoModelMapperService.FromAlgoCard(algoCard, playerIsHost));
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            player.Deck.Cards.AddRange(allCardsInLane);
            DeckService.ShuffleDeck(player.Deck);
            // TODO: Add joker to lane?
            return true;
        }
    }
}
