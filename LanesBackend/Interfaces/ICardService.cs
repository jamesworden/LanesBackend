using LanesBackend.Models;

namespace LanesBackend.Interfaces
{
    public interface ICardService
    {
        public bool RemoveCardWithMatchingKindAndSuit(List<Card> cardList, Card card);
    }
}
