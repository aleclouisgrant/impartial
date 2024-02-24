using System;
using System.Collections.Generic;

namespace Impartial
{
    public interface IFinalCompetition
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public List<IFinalScore> FinalScores { get; set; }

        public Division Division { get; set; }

        public List<ICompetitor> Leaders { get; set; }
        public List<ICompetitor> Followers { get; set; }
        public List<IJudge> Judges { get; set; }

        public List<ICouple> Couples { get; }

        public string ToLongString();
    }
}
