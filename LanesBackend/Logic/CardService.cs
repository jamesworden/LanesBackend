using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class CardService : ICardService
    {
        public bool RemoveCardWithMatchingKindAndSuit(List<Card> cardList, Card card)
        {
            for (int i = 0; i < cardList.Count; i++)
            {
                var cardFromList = cardList[i];
                bool sameSuit = cardFromList.Suit.Equals(card.Suit);
                bool sameKind = cardFromList.Kind.Equals(card.Kind);

                if (sameSuit && sameKind)
                {
                    cardList.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
    }
}
