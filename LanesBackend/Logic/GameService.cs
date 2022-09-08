using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class GameService : IGameService
    {
        private readonly IDeckService DeckService;

        public GameService(IDeckService deckService)
        {
            DeckService = deckService;
        }

        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            var deck = DeckService.CreateAndShuffleDeck();
            var playerDecks = DeckService.SplitDeck(deck);

            var hostDeck = playerDecks.Item1;
            var guestDeck = playerDecks.Item2;

            Game game = new(hostConnectionId, guestConnectionId, gameCode, hostDeck, guestDeck);

            return game;
        }
    }
}
