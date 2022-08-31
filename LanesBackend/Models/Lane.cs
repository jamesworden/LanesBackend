namespace LanesBackend.Models
{
    public class Lane
    {
        public readonly List<Card>[] Rows = new List<Card>[7];

        public Lane()
        { 
            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new List<Card>();
            }
        }
    }
}
