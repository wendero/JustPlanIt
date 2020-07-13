using System;

namespace JustPlanIt.Models
{
    public class Member
    {
        public Member()
        {
            this.Identifier = new Random().Next(100000, 999999);
        }
        public int Identifier { get; set; }
        public string Name { get; set; }
        public bool Leader {get; set; } = false;
    }
}