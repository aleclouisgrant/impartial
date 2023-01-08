using iText.Barcodes.Dmcode;
using System;

namespace Impartial
{
    public class Score
    {
        public Guid Id { get; set; }
        public Competition Competition { get; set; }
        public Judge Judge { get; set; }
        public Competitor Leader { get; set; }
        public Competitor Follower { get; set; }

        public int Placement { get; set; }
        public int ActualPlacement { get; set; }

        public double Accuracy => Util.GetAccuracy(Placement, ActualPlacement);

        public Score(Competition competition, Judge judge, Competitor leader, Competitor follower, int placement, int actualPlacement)
        {
            Id = Guid.NewGuid();

            Competition = competition;
            Judge = judge; Leader = leader; Follower = follower;
            Placement = placement; ActualPlacement = actualPlacement;
        }

        public Score(Judge judge, int placement, int actualPlacement)
        {
            Id = Guid.NewGuid(); 
            
            Judge = judge; 
            Placement = placement; ActualPlacement = actualPlacement;
        }

        public Score
            (Guid id, 
            Guid competitionId, string competitionName, DateTime competitionDate, string division,
            Guid judgeId, string judgeFirstName, string judgeLastName, double judgeAccuracy, double judgeTop5Accuracy,
            Guid leaderId, int leaderWsdcId, string leaderFirstName, string leaderLastName, int leaderRating, double leaderVariance, int leaderFollowerRating, double leaderFollowerVariance,
            Guid followerId, int followerWsdcId, string followerFirstName, string followerLastName, int followerLeaderRating, double followerLeaderVariance, int followerRating, double followerVariance,
            int placement, int actualPlacement)
        { 
            Id = id; 
            Placement = placement;
            ActualPlacement = actualPlacement;

            Competition = new Competition(competitionId, competitionName, competitionDate, Division.AllStar);
            Judge = new Judge(judgeId, judgeFirstName, judgeLastName, judgeAccuracy, judgeTop5Accuracy);
            Leader = new Competitor(leaderId, leaderWsdcId, leaderFirstName, leaderLastName, leaderRating, leaderVariance, leaderFollowerRating, leaderFollowerVariance);
            Follower = new Competitor(followerId, followerWsdcId, followerFirstName, followerLastName, followerLeaderRating, followerLeaderVariance, followerRating, followerVariance);
        }
    }
}
