using LanesBackend.Interfaces;
using LanesBackend.Models;

namespace LanesBackend.Logic
{
    public class LanesService : ILanesService
    {
        private readonly int NUMBER_OF_LANES = 5;

        private readonly int NUMBER_OF_ROWS = 7;

        public Lane[] CreateEmptyLanes()
        {
            Lane[] lanes = new Lane[NUMBER_OF_LANES];

            for (int i = 0; i < lanes.Length; i++)
            {
                lanes[i] = CreateEmptyLane();
            }

            return lanes;
        }

        private Lane CreateEmptyLane()
        {
            var rows = CreateEmptyRows();
            Lane lane = new(rows);

            return lane;
        }

        private List<Card>[] CreateEmptyRows()
        {
            List<Card>[] rows = new List<Card>[NUMBER_OF_ROWS];

            for (int i = 0; i < rows.Length; i++)
            {
                var row = new List<Card>();
                rows[i] = row;
            }

            return rows;
        }
    }
}
