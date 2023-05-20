using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Mappers
{
    public class PlayerGameViewMapper : IPlayerGameViewMapper
    {
        public PlayerGameView MapToHostPlayerGameView(Game game)
        {
            return new PlayerGameView(
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand.Cards.Count,
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand,
                game.Lanes,
                true,
                game.IsHostPlayersTurn,
                game.RedJokerLaneIndex,
                game.BlackJokerLaneIndex,
                game.GameCreatedTimestampUTC,
                game.MovesMade,
                game.DurationOption,
                game.GameEndedTimestampUTC
                );
        }

        public PlayerGameView MapToGuestPlayerGameView(Game game)
        {
            return new PlayerGameView(
                game.HostPlayer.Deck.Cards.Count,
                game.HostPlayer.Hand.Cards.Count,
                game.GuestPlayer.Deck.Cards.Count,
                game.GuestPlayer.Hand,
                game.Lanes,
                false,
                game.IsHostPlayersTurn,
                game.RedJokerLaneIndex,
                game.BlackJokerLaneIndex,
                game.GameCreatedTimestampUTC,
                game.MovesMade,
                game.DurationOption,
                game.GameEndedTimestampUTC
                );
        }
    }
}
