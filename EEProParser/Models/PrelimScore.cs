using Impartial.Enums;
using System;

namespace Impartial
{
    public class PrelimScore
    {
        public Guid Id { get; }
        public Competition Competition { get; set; }
        public Judge Judge { get; set; } 
        public Competitor Competitor { get; set; }
        
        public Role Role { get; set; }
        public bool Finaled { get; set; }
        public CallbackScore CallbackScore { get; set; }
        public int RawScore { get; set; }
        public int Round { get; set; }

        public PrelimScore(Judge judge, Competitor competitor, Role role, bool finaled, CallbackScore callbackScore, int rawScore, int round)
        {
            Id = Guid.NewGuid();

            Judge = judge;
            Competitor = competitor;
            Role = role;
            Finaled = finaled;
            CallbackScore = callbackScore;
            RawScore = rawScore;
            Round = round;
        }

        public PrelimScore(Competition competition, Judge judge, Competitor competitor, Role role, bool finaled, CallbackScore callbackScore, int rawScore, int round)
        {
            Id = Guid.NewGuid();

            Competition = competition;
            Judge = judge;
            Competitor = competitor;
            Role = role;
            Finaled = finaled;
            CallbackScore = callbackScore;
            RawScore = rawScore;
            Round = round;
        }

        public PrelimScore(
            Guid id,
            Guid competitionId, string competitionName, DateTime competitionDate, string division,
            Guid judgeId, string judgeFirstName, string judgeLastName, double judgeAccuracy, double judgeTop5Accuracy,
            Guid competitorId, int competitorWsdcId, string competitorFirstName, string competitorLastName, int competitorLeaderRating, double competitorLeaderVariance, int competitorFollowerRating, double competitorFollowerVariance,
            string role, 
            bool finaled, 
            string callbackScore,
            int rawScore,
            int round)
        {
            Id = id;
            Role = Util.StringToRole(role);
            Finaled = finaled;
            CallbackScore = Util.StringToCallbackScore(callbackScore);
            RawScore = rawScore;
            Round = round;

            Competition = new Competition(competitionId, competitionName, competitionDate, Division.AllStar);
            Judge = new Judge(judgeId, judgeFirstName, judgeLastName, judgeAccuracy, judgeTop5Accuracy);
            Competitor = new Competitor(competitorId, competitorWsdcId, competitorFirstName, competitorLastName, competitorLeaderRating, competitorLeaderVariance, competitorFollowerRating, competitorFollowerVariance);
        }
    }
}
