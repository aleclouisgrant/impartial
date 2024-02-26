using System;
using System.Collections.Generic;

namespace Impartial
{
    public interface ICompetition
    {
        public Guid Id { get; }
        public Division Division { get; set; }

        public Guid DanceConventionId { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public Tier LeaderTier { get; }

        public Tier FollowerTier { get; }

        public List<IPairedPrelimCompetition> PairedPrelimCompetitions { get; set; }
        public IFinalCompetition FinalCompetition { get; set; }

        public string ToString();
        public string ToLongString();

        public void Clear();
        public void ClearFinals();
        public void ClearPrelims();
    }

    public interface IPairedPrelimCompetition
    {
        public Round Round { get; set; }
        public IPrelimCompetition LeaderPrelimCompetition { get; set; }
        public IPrelimCompetition FollowerPrelimCompetition { get; set; }
    }
}
