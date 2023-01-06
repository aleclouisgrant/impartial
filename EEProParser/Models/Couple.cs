using System.Collections.Generic;

namespace Impartial
{
    public class Couple
    {
        public Competitor Leader { get; set; }
        public Competitor Follower { get; set; }
        public int ActualPlacement { get; set; }

        public int CombinedRating => Leader.LeadStats.Rating + Follower.FollowStats.Rating;

        public List<Score> Scores { get; set; } = new List<Score>();

        public Couple(Competitor leader, Competitor follower, int placement)
        {
            Leader = leader;
            Follower = follower;
            ActualPlacement = placement;
        }
    }
}
