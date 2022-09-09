using LanesBackend.Interfaces;
using LanesBackend.Models.GameEngine;

namespace LanesBackend.Logic.GameEngine
{
    public class AlgoLanesService : IAlgoLanesService
    {
        public List<AlgoCard> GrabAllCardsAndClearLane(AlgoLane algoLane)
        {
            List<AlgoCard> cards = new();

            foreach (var row in algoLane.Rows)
            {
                foreach (var card in row)
                {
                    cards.Add(card);
                }
            }

            algoLane.Rows = CreateEmptyRows();

            return cards;
        }

        private List<AlgoCard>[] CreateEmptyRows()
        {
            List<AlgoCard>[] rows = new List<AlgoCard>[7];

            for (int i = 0; i < rows.Length; i++)
            {
                var row = new List<AlgoCard>();
                rows[i] = row;
            }

            return rows;
        }
    }
}
