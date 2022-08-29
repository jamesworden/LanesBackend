using System.Text;

namespace LanesBackend.Models
{
    public class Hand
    {
        public List<Card> Cards { get; set; }

        public Hand(List<Card> cards)
        {
            Cards = cards;
        }

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public void RemoveCard(Card card)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                var cardInHand = Cards[i];
                bool sameSuit = card.Suit.Equals(cardInHand.Suit);
                bool sameKind = card.Kind.Equals(cardInHand.Kind);

                if (sameSuit && sameKind)
                {
                    Cards.RemoveAt(i);
                }
            }
        }
    }
}