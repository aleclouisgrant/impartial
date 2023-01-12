using Impartial.Enums;
using System;

namespace Impartial
{
    public class PrelimScore
    {
        public Guid Id { get; }
        public Competition Competition { get; set; }
        public Judge Judge { get; set; } 
        public Competitor Competitor { get; set; }
        
        public Role Role { get; set; }
        public bool Finaled { get; set; }
        public CallbackScore CallbackScore { get; set; }

        public PrelimScore()
        {
            Id = Guid.NewGuid();
        }
    }
}
