using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface IDeckService
    {
        public Deck CreateAndShuffleDeck();

        public Tuple<Deck, Deck> SplitDeck(Deck deck);
    }
}
