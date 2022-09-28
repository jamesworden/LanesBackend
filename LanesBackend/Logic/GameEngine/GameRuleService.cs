using LanesBackend.Interfaces;
using LanesBackend.Interfaces.GameEngine;
using LanesBackend.Models;

namespace LanesBackend.Logic.GameEngine
{
    public class GameRuleService : IGameRuleService
    {
        private readonly ILanesService LanesService;

        private readonly IDeckService DeckService;

        private readonly IMoveChecksService MoveChecksService;

        public GameRuleService(
            ILanesService lanesService, 
            IDeckService deckService,
            IMoveChecksService moveChecksService)
        {
            LanesService = lanesService;
            DeckService = deckService;
            MoveChecksService = moveChecksService;
        }

        public bool CaptureMiddleIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
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
            DeckService.ShuffleDeck(player.Deck);

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
            else {
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

        public bool TriggerAceRuleIfAppropriate(Lane lane, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var playerPlayedAnAce = placeCardAttempt.Card.Kind == Kind.Ace;
            if (!playerPlayedAnAce)
            {
                return false;
            }

            // Move this fn from move checks service to lanes service? Def no move checks in this class.
            var opponentAceOnTopOfAnyRow = MoveChecksService.OpponentAceOnTopOfAnyRow(lane, playerIsHost);
            if (!opponentAceOnTopOfAnyRow)
            {
                return false;
            }

            _ = LanesService.GrabAllCardsAndClearLane(lane);
            lane.LastCardPlayed = null;
            lane.LaneAdvantage = PlayerOrNone.None;

            return true;
        }

        public bool WinLaneIfAppropriate(Game game, PlaceCardAttempt placeCardAttempt, bool playerIsHost)
        {
            var placeCardInLastRow = playerIsHost ?
                placeCardAttempt.TargetRowIndex == 6 :
                placeCardAttempt.TargetRowIndex == 0;

            if (!placeCardInLastRow)
            {
                return false;
            }

            var lane = game.Lanes[placeCardAttempt.TargetLaneIndex];

            lane.WonBy = playerIsHost ? PlayerOrNone.Host : PlayerOrNone.Guest;
            var allCardsInLane = LanesService.GrabAllCardsAndClearLane(lane);
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            player.Deck.Cards.AddRange(allCardsInLane);
            DeckService.ShuffleDeck(player.Deck);
            
            // Set first won lane to red joker, second to black joker.
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

        public bool WinGameIfAppropriate(Game game)
        {
            var hostWon = game.Lanes.Where(lane => lane.WonBy == PlayerOrNone.Host).Count() == 2;
            if (hostWon)
            {
                game.WonBy = PlayerOrNone.Host;
                game.isRunning = false;
                return true;
            }

            var guestWon = game.Lanes.Where(lane => lane.WonBy == PlayerOrNone.Guest).Count() == 2;
            if (hostWon)
            {
                game.WonBy = PlayerOrNone.Guest;
                game.isRunning = false;
                return true;
            }

            return false;
        }
    }
}
