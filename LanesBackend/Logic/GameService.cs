using LanesBackend.Interfaces;
using LanesBackend.Models;
using LanesBackend.Utils;

namespace LanesBackend.Logic
{
    public class GameService : IGameService
    {
        private readonly IDeckService DeckService;

        private readonly ILanesService LanesService;

        private readonly IGameEngineService GameEngineService;

        public GameService(IDeckService deckService, ILanesService lanesService, IGameEngineService gameEngineService)
        {
            DeckService = deckService;
            LanesService = lanesService;
            GameEngineService = gameEngineService;
        }

        public Game CreateGame(string hostConnectionId, string guestConnectionId, string gameCode)
        {
            var deck = DeckService.CreateAndShuffleDeck();
            var playerDecks = DeckService.SplitDeck(deck);

            var hostDeck = playerDecks.Item1;
            var guestDeck = playerDecks.Item2;

            var hostHandCards = DeckService.DrawCards(hostDeck, 5);
            var guestHandCards = DeckService.DrawCards(guestDeck, 5);

            var hostHand = new Hand(hostHandCards);
            var guestHand = new Hand(guestHandCards);

            var hostPlayer = new Player(hostDeck, hostHand);
            var guestPlayer = new Player(guestDeck, guestHand);

            var lanes = LanesService.CreateEmptyLanes();

            Game game = new(hostConnectionId, guestConnectionId, gameCode, hostPlayer, guestPlayer, lanes);

            return game;
        }

        public bool MakeMoveIfValid(Game game, Move move, string playerConnectionId)
        {
            var playerIsHost = game.HostConnectionId == playerConnectionId;

            var moveIsValid = GameEngineService.MoveIsValid(game, move, playerIsHost);

            if (moveIsValid)
            {
                GameEngineService.MakeMove(game, move, playerIsHost);
            }

            return moveIsValid;
        }
    }
}
