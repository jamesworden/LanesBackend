namespace LanesBackend.Models
{
    public class Lane
    {
        public readonly Stack<Card>[] Rows = new Stack<Card>[7];

        public Lane()
        { 
            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new Stack<Card>();
            }
        }
    }
}
