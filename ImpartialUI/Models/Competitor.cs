using Impartial;
using System;

namespace ImpartialUI.Models
{
    public class Competitor : UserBase, ICompetitor
    {
        public new string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;

        public Guid CompetitorId { get; set; }
        public int WsdcId { get; set; }

        public RoleStats LeadStats { get; set; } = new RoleStats();
        public RoleStats FollowStats { get; set; } = new RoleStats();

        public Competitor(string firstName, string lastName, int wsdcId = 0)
        {
            UserId = Guid.NewGuid();
            CompetitorId = Guid.NewGuid();

            FirstName = firstName;
            LastName = lastName;
            WsdcId = wsdcId;
        }

        public Competitor(Guid userId, Guid competitorId, int wsdcId, string firstName, string lastName, int leaderRating, double leaderVariance, int followerRating, double followerVariance)
        {
            UserId = userId;
            CompetitorId = competitorId;
            FirstName = firstName;
            LastName = lastName;
            WsdcId = wsdcId;

            LeadStats = new RoleStats(leaderRating, leaderVariance);
            FollowStats = new RoleStats(followerRating, followerVariance);
        }
    }
}
