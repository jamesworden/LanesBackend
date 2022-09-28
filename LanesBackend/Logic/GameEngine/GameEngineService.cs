using LanesBackend.Interfaces;
using LanesBackend.Interfaces.GameEngine;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class GameEngineService : IGameEngineService
    {
        private readonly IMoveChecksService MoveChecksService;

        private readonly IGameRuleService GameRuleService;

        public GameEngineService(
            IMoveChecksService moveChecksService,
            IDeckService deckService,
            IGameRuleService gameRuleService)
        {
            MoveChecksService = moveChecksService;
            GameRuleService = gameRuleService;
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

            // For now assume all moves are one place card attempt.
            var placeCardAttempt = move.PlaceCardAttempts[0];
            var targetLaneIndex = placeCardAttempt.TargetLaneIndex;
            var lane = game.Lanes[targetLaneIndex];

            var laneWon = lane.WonBy != PlayerOrNone.None;
            if (laneWon)
            {
                Console.WriteLine("Client broke the rules: Tried to move a lane that has been won.");
                return false;
            }
            
            var moveStartsOnPlayerSide = playerIsHost ? 
                placeCardAttempt.TargetRowIndex < 3 :
                placeCardAttempt.TargetRowIndex > 3;
            var playerHasAdvantage = playerIsHost ?
                lane.LaneAdvantage == PlayerOrNone.Host :
                lane.LaneAdvantage == PlayerOrNone.Guest;
            if (moveStartsOnPlayerSide && playerHasAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their own side when they have the advantage.");
                return false;
            }

            var moveStartsOnOpponentSide = playerIsHost ?
                placeCardAttempt.TargetRowIndex > 3 :
                placeCardAttempt.TargetRowIndex < 3;
            var opponentHasAdvantage = playerIsHost ?
                 lane.LaneAdvantage == PlayerOrNone.Guest :
                 lane.LaneAdvantage == PlayerOrNone.Host;
            if (moveStartsOnOpponentSide && opponentHasAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their opponent's side when they have their opponent has the advantage.");
                return false;
            }

            var noAdvantage = lane.LaneAdvantage == PlayerOrNone.None;
            if (moveStartsOnOpponentSide && noAdvantage)
            {
                Console.WriteLine("Client broke the rules: Tried to move on their opponent's side when there is no advantage.");
                return false;
            }

            // TODO: Make sure not only the rows are occupied but the top card of the row was last played by player
            var playerSideCardsInOrder = playerIsHost ?
                MoveChecksService.AllPreviousRowsOccupied(lane, placeCardAttempt.TargetRowIndex) :
                MoveChecksService.AllFollowingRowsOccupied(lane, placeCardAttempt.TargetRowIndex);
            if (moveStartsOnPlayerSide && !playerSideCardsInOrder)
            {
                Console.WriteLine("Client broke the rules: Tried to move on position where previous rows aren't occupied.");
                return false;
            }

            var cardIsAce = placeCardAttempt.Card.Kind == Kind.Ace;
            var opponentAceOnTopOfAnyRow = MoveChecksService.OpponentAceOnTopOfAnyRow(lane, playerIsHost);
            var playedAceToNukeRow = cardIsAce && opponentAceOnTopOfAnyRow;
            var cardsHaveMatchingSuitOrKind =
                lane.LastCardPlayed is not null &&
                (placeCardAttempt.Card.Suit == lane.LastCardPlayed.Suit ||
                placeCardAttempt.Card.Kind == lane.LastCardPlayed.Kind);
            if (lane.LastCardPlayed is not null && !cardsHaveMatchingSuitOrKind && !playedAceToNukeRow)
            {
                Console.WriteLine("Client broke the rules: Tried to play a card that has other suit or other kind than the last card played OR not an ace to nuke the row.");
                return false;
            }

            var targetRow = lane.Rows[placeCardAttempt.TargetRowIndex];
            var targetCard = targetRow.Any() ? targetRow.Last() : null;
            var playerPlayedTargetCard = targetCard is not null &&
                (playerIsHost ?
                targetCard.PlayedBy == PlayerOrNone.Host :
                targetCard.PlayedBy == PlayerOrNone.Guest);
            if (
              targetCard is not null &&
              targetCard.Suit != placeCardAttempt.Card.Suit &&
              playerPlayedTargetCard
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a different suit.");
                return false;
            }

            if (
              targetCard is not null &&
              playerPlayedTargetCard &&
              !MoveChecksService.CardTrumpsCard(placeCardAttempt.Card, targetCard)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to reinforce with a lesser card.");
                return false;
            }

            if (
              targetCard is not null &&
              targetCard.Suit == placeCardAttempt.Card.Suit &&
              !MoveChecksService.CardTrumpsCard(placeCardAttempt.Card, targetCard)
            )
            {
                Console.WriteLine("Client broke the rules: Tried to capture a lesser card.");
                return false;
            }

            return true;
        }

        public void MakeMove(Game game, Move move, bool playerIsHost)
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
        }

        private void PlaceCardAndApplyGameRules(Game game, PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsHost)
        {
            var aceRuleTriggered = GameRuleService.TriggerAceRuleIfAppropriate(lane, placeCardAttempt, playerIsHost);

            if (aceRuleTriggered)
            {
                return;
            }

            PlaceCard(lane, placeCardAttempt, playerIsHost);

            var middleCaptured = GameRuleService.CaptureMiddleIfAppropriate(game, placeCardAttempt, playerIsHost);

            if (middleCaptured)
            {
                return;
            }

            GameRuleService.WinLaneIfAppropriate(game, placeCardAttempt, playerIsHost);

            GameRuleService.WinGameIfAppropriate(game);
        }

        private static void PlaceCard(Lane lane, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var targetRow = lane.Rows[placeCardAttempt.TargetRowIndex];
            placeCardAttempt.Card.PlayedBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            targetRow.Add(placeCardAttempt.Card);
            lane.LastCardPlayed = placeCardAttempt.Card;
        }
    }
}
