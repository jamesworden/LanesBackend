using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class DeckService : IDeckService
    {
        public Deck CreateAndShuffleDeck()
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

            deck.Shuffle();

            return deck;
        }

        public Tuple<Deck, Deck> SplitDeck(Deck deck)
        {
            var firstDeckCards = deck.DrawCards(26);
            var firstDeck = new Deck(firstDeckCards);

            var secondDeckCards = deck.DrawRemainingCards();
            var secondDeck = new Deck(secondDeckCards);

            var decks = new Tuple<Deck, Deck>(firstDeck, secondDeck);

            return decks;
        }
    }
}
