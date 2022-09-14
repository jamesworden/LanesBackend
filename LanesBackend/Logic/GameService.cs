using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class GameService : IGameService
    {
        private readonly IDeckService DeckService;

        private readonly ILanesService LanesService;

        private readonly IGameEngineService GameEngineService;

        private readonly ICardService CardService;

        public GameService(
            IDeckService deckService,
            ILanesService lanesService,
            IGameEngineService gameEngineService, 
            ICardService cardService)
        {
            DeckService = deckService;
            LanesService = lanesService;
            GameEngineService = gameEngineService;
            CardService = cardService;
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

        public bool MakeMoveIfValid(Game game, Move move, bool playerIsHost)
        {
            var moveIsValid = GameEngineService.MoveIsValid(game, move, playerIsHost);

            if (!moveIsValid)
            {
                // TODO: Player commited illegal move; end game and penalize player.
                return false;
            }

            GameEngineService.MakeMove(game, move, playerIsHost);

            RemoveCardsFromHand(game, playerIsHost, move);

            DrawCardFromDeck(game, playerIsHost);

            return true;
        }

        public void RemoveCardsFromHand(Game game, bool playerIsHost, Move move)
        {
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            // For now assume all moves are one place card attempt.
            var card = move.PlaceCardAttempts[0].Card;

            CardService.RemoveCardWithMatchingKindAndSuit(player.Hand.Cards, card);
        }

        public void DrawCardFromDeck(Game game, bool playerIsHost)
        {
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            var playersDeckHasCards = player.Deck.Cards.Any();

            if (!playersDeckHasCards)
            {
                return;
            }

            var cardFromDeck = DeckService.DrawCard(player.Deck);

            if (cardFromDeck is not null)
            {
                player.Hand.AddCard(cardFromDeck);
            }
        }
    }
}
