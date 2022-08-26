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
            Cards.Remove(card);
        }
    }
}