using System;
using System.Collections.Generic;

namespace Impartial
{
    public interface IPrelimCompetition
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public List<IPrelimScore> PrelimScores { get; set; }

        public Division Division { get; set; }
        public Round Round { get; set; }
        public Role Role { get; set; }

        public List<ICompetitor> Competitors { get; set; }
        public List<IJudge> Judges { get; set; }

        public List<ICompetitor> PromotedCompetitors { get; set; }

        public string ToLongString();        
    }
}
