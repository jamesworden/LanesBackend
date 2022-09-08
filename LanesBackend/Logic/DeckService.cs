using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class DeckService : IDeckService
    {
        public Deck CreateAndShuffleDeck()
        {
            var deck = CreateDeck();
            deck = ShuffleDeck(deck);

            return deck;
        }

        public Tuple<Deck, Deck> SplitDeck(Deck deck)
        {
            var numCardsInHalfDeck = deck.Cards.Count / 2;

            var firstDeckCards = DrawCards(deck, numCardsInHalfDeck);
            var firstDeck = new Deck(firstDeckCards);

            var secondDeckCards = DrawRemainingCards(deck);
            var secondDeck = new Deck(secondDeckCards);

            var decks = new Tuple<Deck, Deck>(firstDeck, secondDeck);

            return decks;
        }

        public List<Card> DrawCards(Deck deck, int numberOfCards)
        {
            List<Card> cards = new();

            if (numberOfCards > deck.Cards.Count)
            {
                numberOfCards = deck.Cards.Count;
            }

            for (int i = 0; i < numberOfCards; i++)
            {
                var card = deck.Cards.ElementAt(i);
                deck.Cards.RemoveAt(i);
                cards.Add(card);
            }

            return cards;
        }

        public Deck ShuffleDeck(Deck deck)
        {
            Random random = new();
            deck.Cards = deck.Cards.OrderBy(card => random.Next()).ToList();

            return deck;
        }

        public Card? DrawCard(Deck deck)
        {
            List<Card> singleCardList = DrawCards(deck, 1);

            if (singleCardList.Count != 1)
            {
                return null;
            }
            
            var card = singleCardList.ElementAt(0);

            return card;
        }

        public List<Card> DrawRemainingCards(Deck deck)
        {
            List<Card> remainingCards = new(deck.Cards);
            deck.Cards.Clear();

            return remainingCards;
        }

        private static Deck CreateDeck()
        {
            var cards = new List<Card>();

            var suits = Enum.GetValues(typeof(Suit));

            foreach (Suit suit in suits)
            {
                var kinds = Enum.GetValues(typeof(Kind));

                foreach (Kind kind in kinds)
                {
                    var card = new Card(kind, suit);
                    cards.Add(card);
                }
            }

            var deck = new Deck(cards);

            return deck;
        }
    }
}
