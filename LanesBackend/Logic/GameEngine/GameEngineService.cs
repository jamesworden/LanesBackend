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

        public GameEngineService(
            IMoveChecksService moveChecksService,
            IAlgoModelMapperService algoModelMapperService,
            IAlgoMoveChecksService algoMoveChecksService)
        {
            MoveChecksService = moveChecksService;
            AlgoModelMapperService = algoModelMapperService;
            AlgoMoveChecksService = algoMoveChecksService;
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

            var targetLaneIndex = move.PlaceCardAttempts[0].TargetLaneIndex;
            var lane = game.Lanes[targetLaneIndex];

            var laneWon = lane.WonBy != PlayerOrNone.None;
            if (laneWon)
            {
                Console.WriteLine("Client broke the rules: Tried to move a lane that has been won.");
                return false;
            }

            var algoLane = AlgoModelMapperService.ToAlgoLane(lane, playerIsHost);
            var algoMove = AlgoModelMapperService.ToAlgoMove(move, playerIsHost);

            return true;
        }

        public bool MakeMove(Game game, Move move, bool playerIsHost)
        {
            throw new NotImplementedException();
        }

        //private void MakeMoveFromHostPov(Game game, Move hostPovMove, Lane hostPovLane, bool playerIsTruelyHost)
        //{
        //    var isPairMove = hostPovMove.PlaceCardAttempts.Count > 1;

        //    if (!isPairMove)
        //    {
        //        game.IsHostPlayersTurn = !game.IsHostPlayersTurn;
        //    }

        //    foreach (var placeCardAttempt in hostPovMove.PlaceCardAttempts)
        //    {
        //        PlaceCardFromHostPovAndApplyGameRules(game, placeCardAttempt, hostPovLane, playerIsTruelyHost);
        //    }
        //}

        //private void PlaceCardFromHostPovAndApplyGameRules(Game game, PlaceCardAttempt placeCardAttempt, Lane targetLane, bool playerIsTruelyHost)
        //{
        //    var aceRuleTriggered = TriggerAceRuleIfAppropriateFromHostPov(placeCardAttempt, targetLane);

        //    if (aceRuleTriggered)
        //    {
        //        return;
        //    }

        //    PlaceCardFromHostPov(targetLane, placeCardAttempt);

        //    var middleCaptured = CaptureMiddleIfAppropriateFromHostPov(game, placeCardAttempt, targetLane, playerIsTruelyHost);

        //    if (middleCaptured)
        //    {
        //        return;
        //    }

        //    _ = WinLaneIfAppropriateFromHostPov(game, placeCardAttempt, targetLane, playerIsTruelyHost);
        //}

        //private void PlaceCardFromHostPov(Lane lane, PlaceCardAttempt placeCardAttempt)
        //{
        //    var targetRow = lane.Rows[placeCardAttempt.TargetRowIndex];
        //    placeCardAttempt.Card.PlayedBy = PlayerOrNone.Host;
        //    targetRow.Add(placeCardAttempt.Card);
        //    lane.LastCardPlayed = placeCardAttempt.Card;
        //}

        //private bool CaptureMiddleIfAppropriateFromHostPov(Game game, PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsTruelyHost)
        //{
        //    var cardIsLastOnHostSide = placeCardAttempt.TargetRowIndex == 2;

        //    if (!cardIsLastOnHostSide)
        //    {
        //        return false;
        //    }

        //    if (lane.LaneAdvantage == PlayerOrNone.None)
        //    {
        //        CaptureNoAdvantageLane(lane, placeCardAttempt);
        //    }
        //    else if (lane.LaneAdvantage == PlayerOrNone.Guest)
        //    {
        //        CaptureOpponentAdvantageLane(game, lane, playerIsTruelyHost);
        //    }

        //    return true;
        //}

        //private void CaptureNoAdvantageLane(Lane lane, PlaceCardAttempt placeCardAttempt)
        //{
        //    var cardsFromLane = LanesService.GrabAllCardsAndClearLane(lane);

        //    // Put last placed card at top of pile
        //    cardsFromLane.Remove(placeCardAttempt.Card);
        //    cardsFromLane.Add(placeCardAttempt.Card);

        //    var middleRow = lane.Rows[3];
        //    middleRow.AddRange(cardsFromLane);
        //    lane.LaneAdvantage = PlayerOrNone.Host;
        //}

        //private void CaptureOpponentAdvantageLane(Game game, Lane lane, bool playerIsTruelyHost)
        //{
        //    List<Card> topCardsOfFirstThreeRows = new();

        //    for (int i = 0; i < 3; i++)
        //    {
        //        var card = lane.Rows[i].TakeLast(1).FirstOrDefault();

        //        if (card is not null)
        //        {
        //            topCardsOfFirstThreeRows.Add(card);
        //        }
        //    }

        //    var remainingCardsInLane = LanesService.GrabAllCardsAndClearLane(lane);

        //    var middleRow = lane.Rows[3];
        //    middleRow.AddRange(topCardsOfFirstThreeRows);

        //    var player = playerIsTruelyHost ? game.HostPlayer : game.GuestPlayer;
        //    player.Deck.Cards.AddRange(remainingCardsInLane);

        //    DeckService.ShuffleDeck(player.Deck);

        //    lane.LaneAdvantage = PlayerOrNone.Host;
        //}

        //private bool TriggerAceRuleIfAppropriateFromHostPov(PlaceCardAttempt placeCardAttempt, Lane lane)
        //{
        //    var playerPlayedAnAce = placeCardAttempt.Card.Kind == Kind.Ace;

        //    if (!playerPlayedAnAce)
        //    {
        //        return false;
        //    }

        //    var opponentAceOnTopCardOfAnyRow = false;

        //    for (int i = 0; i < lane.Rows.Length; i++)
        //    {
        //        var topCard = LaneUtils.GetTopCardOfTargetRow(lane, i);

        //        if (topCard is null)
        //        {
        //            continue;
        //        }

        //        if (topCard.PlayedBy == PlayerOrNone.Guest && topCard.Kind == Kind.Ace)
        //        {
        //            opponentAceOnTopCardOfAnyRow = true;
        //            break;
        //        }
        //    }

        //    if (!opponentAceOnTopCardOfAnyRow)
        //    {
        //        return false;
        //    }

        //    _ = LanesService.GrabAllCardsAndClearLane(lane);

        //    return true;
        //}

        //private bool WinLaneIfAppropriateFromHostPov(Game game, PlaceCardAttempt placeCardAttempt, Lane lane, bool playerIsTruelyHost)
        //{
        //    var placeCardInLastRow = placeCardAttempt.TargetRowIndex == 6;

        //    if (!placeCardInLastRow)
        //    {
        //        return false;
        //    }

        //    lane.WonBy = PlayerOrNone.Host;
        //    var allCardsInLane = LanesService.GrabAllCardsAndClearLane(lane);
        //    var player = playerIsTruelyHost ? game.HostPlayer : game.GuestPlayer;
        //    player.Deck.Cards.AddRange(allCardsInLane);
        //    DeckService.ShuffleDeck(player.Deck);
        //    // TODO: Add joker to lane?
        //    return true;
        //}
    }
}
