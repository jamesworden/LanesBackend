﻿namespace LanesBackend.Models
{
    public class Lane
    {
        public List<Card>[] Rows = new List<Card>[7];

        public LaneAdvantage LaneAdvantage = LaneAdvantage.None;

        public Card? LastCardPlayed;

        public Lane()
        { 
            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i] = new List<Card>();
            }
        }
    }
}
