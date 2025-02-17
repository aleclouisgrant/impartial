using Impartial;
using System.Collections.Generic;

namespace ImpartialUI.Models
{
    public class Couple : ICouple
    {
        public ICompetitorRegistration LeaderRegistration { get; set; }
        public ICompetitorRegistration FollowerRegistration { get; set; }

        private ICompetitor _leader;
        public ICompetitor Leader
        {
            get
            {
                return LeaderRegistration == null ? _leader : LeaderRegistration.Competitor;
            }
            set
            {
                if (LeaderRegistration == null)
                {
                    _leader = value;
                }
                else
                {
                    LeaderRegistration.Competitor = value;
                }
            }
        }

        private ICompetitor _follower;
        public ICompetitor Follower
        {
            get
            {
                return FollowerRegistration == null ? _follower : FollowerRegistration.Competitor;
            }
            set
            {
                if (FollowerRegistration == null)
                {
                    _follower = value;
                }
                else
                {
                    FollowerRegistration.Competitor = value;
                }
            }
        }
        public int Placement { get; set; }

        public int CombinedRating => Leader.LeadStats.Rating + Follower.FollowStats.Rating;

        public List<IFinalScore> Scores { get; set; } = new();

        public Couple(ICompetitor leader, ICompetitor follower, int placement)
        {
            Leader = leader;
            Follower = follower;
            Placement = placement;
        }

        public Couple(ICompetitorRegistration leaderRegistration, ICompetitorRegistration followerRegistration, int placement, List<IFinalScore> scores = null)
        {
            LeaderRegistration = leaderRegistration;
            FollowerRegistration = followerRegistration;
            Placement = placement;

            Scores = scores ?? new List<IFinalScore>();
        }
    }
}