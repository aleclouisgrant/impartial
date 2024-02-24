using System.Collections.Generic;

namespace Impartial
{
    public interface ICouple
    {
        public ICompetitor Leader { get; set; }
        public ICompetitor Follower { get; set; }
        public int ActualPlacement { get; set; }

        public int CombinedRating => Leader.LeadStats.Rating + Follower.FollowStats.Rating;

        public List<IFinalScore> Scores { get; set; }
    }
}
