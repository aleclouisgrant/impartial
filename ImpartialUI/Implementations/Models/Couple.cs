using Impartial;
using System.Collections.Generic;

namespace ImpartialUI
{
    public class Couple : ICouple
    {
        public ICompetitor Leader { get; set; }
        public ICompetitor Follower { get; set; }
        public int ActualPlacement { get; set; }

        public int CombinedRating => Leader.LeadStats.Rating + Follower.FollowStats.Rating;

        public List<IFinalScore> Scores { get; set; } = new();

        public Couple(ICompetitor leader, ICompetitor follower, int placement)
        {
            Leader = leader;
            Follower = follower;
            ActualPlacement = placement;
        }
    }
}
