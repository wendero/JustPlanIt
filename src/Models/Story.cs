using System.Collections.Generic;
using System;

namespace JustPlanIt.Models
{
    public class Story
    {
        public Story()
        {
            this.Identifier = new Random().Next(100000, 999999);
        }
        public int Identifier { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Points { get; set; }
        public StoryStatus Status { get; set; } = StoryStatus.Ready;
        public Dictionary<string, int> Votes { get; set; } = new Dictionary<string, int>();
        

    }
    public enum StoryStatus
    {
        Ready = 0,
        Active = 1,
        Voting = 2,
        Voted = 3,
        ShowingResults = 4,
        Closed = 9,
        Done = 10
    }
}