using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface ICardService
    {
        public bool RemoveCardWithMatchingKindAndSuit(List<Card> cardList, Card card);

        public Deck CreateAndShuffleDeck();

        public Tuple<Deck, Deck> SplitDeck(Deck deck);

        public Deck ShuffleDeck(Deck deck);

        public List<Card> DrawCards(Deck deck, int numberOfCards);

        public Card? DrawCard(Deck deck);

        public List<Card> DrawRemainingCards(Deck deck);
    }
}
