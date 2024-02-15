using System;
using System.Collections.Generic;
using System.Linq;

namespace Impartial
{
    public class PairedPrelimCompetition
    {
        public Round Round { get; set; }
        public PrelimCompetition LeaderPrelimCompetition { get; set; }
        public PrelimCompetition FollowerPrelimCompetition { get; set; }
    }

    public class Competition
    {
        public Guid Id { get; }
        public Division Division { get; set; }

        public Guid DanceConventionId { get; set; }
        public string Name { get; }
        public DateTime Date { get; }

        public Tier LeaderTier => PairedPrelimCompetitions.Count > 0 ?
            Util.GetTier(PairedPrelimCompetitions[0].LeaderPrelimCompetition.Competitors.Count) :
            Util.GetTier(FinalCompetition.Leaders.Count);

        public Tier FollowerTier => PairedPrelimCompetitions.Count > 0 ?
            Util.GetTier(PairedPrelimCompetitions[0].FollowerPrelimCompetition.Competitors.Count) :
            Util.GetTier(FinalCompetition.Followers.Count);

        public List<PairedPrelimCompetition> PairedPrelimCompetitions { get; set; } = new();
        public FinalCompetition FinalCompetition { get; set; }
        
        public Competition() 
        { 
            Id = Guid.NewGuid();
        }

        public Competition(Guid danceConventionId, string name, DateTime date, Division division, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            Division = division;

            DanceConventionId = danceConventionId;
            Name = name;
            Date = date;
        }

        public override string ToString()
        {
            return Name + " " + Division + " Jack & Jill (" + Date.ToShortDateString() + ")";
        }
        public string ToLongString()
        {
            string str = ToString();
            
            str += Environment.NewLine;

            foreach (var pairedPrelimCompetition in PairedPrelimCompetitions)
            {
                str += pairedPrelimCompetition.LeaderPrelimCompetition.ToLongString();
                str += Environment.NewLine;
                str += pairedPrelimCompetition.FollowerPrelimCompetition.ToLongString();
            }
            
            str += Environment.NewLine;

            if (FinalCompetition != null) 
            {
                str += FinalCompetition.ToLongString();
            }
            
            return str;
        }

        public void Clear()
        {
            ClearPrelims();
            ClearFinals();
        }
        public void ClearFinals()
        {
            FinalCompetition = null;
        }
        public void ClearPrelims()
        {
            PairedPrelimCompetitions.Clear();
        }
    }
}
