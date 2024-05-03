using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Mappers
{
    public class PlayerGameViewMapper : IPlayerGameViewMapper
    {
        public PlayerGameView MapToHostPlayerGameView(Game game)
        {
            var isHost = true;
            var candidateMoves = (game.IsHostPlayersTurn && game.CandidateMoves.Any()) 
                ? game.CandidateMoves.LastOrDefault() 
                : new List<CandidateMove>();

            return new PlayerGameView(
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand.Cards.Count,
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand,
                game.Lanes,
                isHost,
                game.IsHostPlayersTurn,
                game.RedJokerLaneIndex,
                game.BlackJokerLaneIndex,
                game.GameCreatedTimestampUTC,
                HideOpponentsDrawnCards(game.MovesMade, isHost),
                game.DurationOption,
                game.GameEndedTimestampUTC,
                game.GameCode,
                candidateMoves,
                game.HasEnded
                );
        }

        public PlayerGameView MapToGuestPlayerGameView(Game game)
        {
            var isHost = false;
            var candidateMoves = (!game.IsHostPlayersTurn && game.CandidateMoves.Any())
                ? game.CandidateMoves.LastOrDefault()
                : new List<CandidateMove>();

            return new PlayerGameView(
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand.Cards.Count,
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand,
                game.Lanes,
                isHost,
                game.IsHostPlayersTurn,
                game.RedJokerLaneIndex,
                game.BlackJokerLaneIndex,
                game.GameCreatedTimestampUTC,
                HideOpponentsDrawnCards(game.MovesMade, isHost),
                game.DurationOption,
                game.GameEndedTimestampUTC,
                game.GameCode,
                candidateMoves,
                game.HasEnded
                );
        }

        private List<MoveMade> HideOpponentsDrawnCards(List<MoveMade> movesMade, bool isHost)
        {
            return movesMade.Select(moveMade =>
            {
                var newMoveMade = new MoveMade(moveMade.PlayedBy, moveMade.Move, moveMade.TimestampUTC, new List<List<CardMovement>>())
                {
                    CardMovements = moveMade.CardMovements.Select(movementBurstMade =>
                    {
                        return movementBurstMade.Select(cardMovement =>
                        {
                            var fromHostDeckAndIsGuest = (cardMovement.From.HostDeck && !isHost);
                            var fromGuestDeckAndIsHost = (cardMovement.From.GuestDeck && isHost);
                            var isOpponentDrawnCardMovement = fromHostDeckAndIsGuest || fromGuestDeckAndIsHost;

                            var newCardMovement = new CardMovement(
                                cardMovement.From, 
                                cardMovement.To, 
                                isOpponentDrawnCardMovement ? null : cardMovement.Card, 
                                cardMovement.Notation);

                            return newCardMovement;
                        }).ToList();
                    }).ToList()
                };

                return newMoveMade;
            }).ToList();
        }
    }
}
