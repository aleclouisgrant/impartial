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

        public List<ICompetitor> Competitors { get; }
        public List<IJudge> Judges { get; }

        public List<ICompetitor> PromotedCompetitors { get; set; }

        public string ToLongString();

        public void Clear();
    }
}
