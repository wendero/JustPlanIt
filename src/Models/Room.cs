using System.Collections.Generic;
using System;

namespace JustPlanIt.Models
{
    public class Room
    {
        private static readonly int[] FibonacciDeck = new[]
        {
            1, 2, 3, 5, 8, 13, 21, 100
        };
        public Room()
        {
            this.Identifier = new Random().Next(100000, 999999);
        }
        public int Identifier { get; set; }
        public string Name { get; set; }
        public int[] Deck { get; } = FibonacciDeck;
        public List<Member> Members { get; set; } = new List<Member>();
        public List<Story> Stories { get; set; } = new List<Story>();
        public bool IsClosing { get; set; } = false;
    }
}