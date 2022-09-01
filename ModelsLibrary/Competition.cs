using System;
using System.Collections.Generic;

namespace Impartial
{
    public class Competition
    {
        public Guid Id { get; }

        public Division Division { get; }

        public List<Score> Scores { get; set; }

        public int TotalCouples => Scores?.Count ?? 0;

        public Competition(Division division)
        {
            Division = division;
        }
    }
}
