using Impartial;
using System;
using System.Linq;

namespace ImpartialUI.Models
{
    public class FinalScore : IFinalScore
    {
        public Guid Id { get; set; }
        public IJudge Judge { get; set; }


        public ICompetitorRegistration LeaderRegistration { get; set; }
        public ICompetitorRegistration FollowerRegistration { get; set; }

        public ICompetitor Leader
        {
            get
            {
                return LeaderRegistration?.Competitor;
            }
            set
            {
                if (LeaderRegistration == null)
                {
                    LeaderRegistration = new CompetitorRegistration(value, string.Empty);
                }
                else
                {
                    LeaderRegistration.Competitor = value;
                }
            }
        }

        public ICompetitor Follower
        {
            get
            {
                return FollowerRegistration?.Competitor;
            }
            set
            {
                if (FollowerRegistration == null)
                {
                    FollowerRegistration = new CompetitorRegistration(value, string.Empty);
                }
                else
                {
                    FollowerRegistration.Competitor = value;
                }
            }
        }

        public int Score { get; set; }
        public int Placement { get; set; }

        public double Accuracy => Util.GetAccuracy(Score, Placement);

        public FinalScore(IJudge judge, ICompetitor leader, ICompetitor follower, int score, int placement, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            Judge = judge; Leader = leader; Follower = follower;
            Score = score; Placement = placement;
        }

        public FinalScore(Guid? judgeId, Guid? leaderId, Guid? followerId, int score, int placement, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            Score = score;
            Placement = placement;

            SetJudge(judgeId);
            SetLeader(leaderId);
            SetFollower(followerId);
        }

        public FinalScore(IJudge judge, ICompetitorRegistration leaderRegistration, ICompetitorRegistration followerRegistration, int score, int placement, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            Judge = judge; 
            LeaderRegistration = leaderRegistration; FollowerRegistration = followerRegistration;
            Score = score; Placement = placement;
        }

        public void SetJudge(Guid? id)
        {
            Judge = App.JudgesDb.FirstOrDefault(j => j.JudgeId == id);
        }

        public void SetLeader(Guid? id)
        {
            Leader = App.CompetitorsDb.FirstOrDefault(j => j.CompetitorId == id);
        }

        public void SetFollower(Guid? id)
        {
            Follower = App.CompetitorsDb.FirstOrDefault(j => j.CompetitorId == id);
        }
    }
}
