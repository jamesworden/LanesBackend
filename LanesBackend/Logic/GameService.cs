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
            // TODO: Backend move validation

            //var moveIsValid = GameEngineService.MoveIsValid(game, move, playerIsHost);

            //if (!moveIsValid)
            //{
            //    // TODO: Player commited illegal move; end game and penalize player.
            //    return false;
            //}

            GameEngineService.MakeMove(game, move, playerIsHost);

            var numCardsRemoved = RemoveCardsFromHand(game, playerIsHost, move);

            DrawCardsFromDeck(game, playerIsHost, numCardsRemoved);

            return true;
        }

        public int RemoveCardsFromHand(Game game, bool playerIsHost, Move move)
        {
            var numCardsRemoved = 0;
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;
            
            foreach(var placeCardAttempt in move.PlaceCardAttempts)
            {
                CardService.RemoveCardWithMatchingKindAndSuit(player.Hand.Cards, placeCardAttempt.Card);
                numCardsRemoved++;
            }

            return numCardsRemoved;
        }

        public void DrawCardsFromDeck(Game game, bool playerIsHost, int numCardsToDraw)
        {
            var player = playerIsHost ? game.HostPlayer : game.GuestPlayer;

            for(int i = 0; i < numCardsToDraw; i++)
            {
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
}
