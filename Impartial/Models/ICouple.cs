using System.Collections.Generic;

namespace Impartial
{
    public interface ICouple
    {
        public ICompetitorRegistration LeaderRegistration { get; set; }
        public ICompetitorRegistration FollowerRegistration { get; set; }

        public ICompetitor Leader { get; set; }
        public ICompetitor Follower { get; set; }

        public int Placement { get; set; }

        public int CombinedRating => Leader.LeadStats.Rating + Follower.FollowStats.Rating;

        public List<IFinalScore> Scores { get; set; }
    }
}