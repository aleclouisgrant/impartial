using Impartial;
using System;

namespace ImpartialUI.Models
{
    public class Competitor : PersonModel, ICompetitor
    {
        // personal info
        public new string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;

        public int WsdcId { get; set; }

        public RoleStats LeadStats { get; set; } = new RoleStats();
        public RoleStats FollowStats { get; set; } = new RoleStats();

        public Competitor(string firstName, string lastName)
        {
            Id = Guid.NewGuid();

            FirstName = firstName;
            LastName = lastName;
        }

        public Competitor(string firstName, string lastName, int wsdcId)
        {
            Id = Guid.NewGuid();

            FirstName = firstName;
            LastName = lastName;
            WsdcId = wsdcId;
        }

        public Competitor(Guid id, int wsdcId, string firstName, string lastName, int leaderRating, double leaderVariance, int followerRating, double followerVariance)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            WsdcId = wsdcId;

            LeadStats = new RoleStats(leaderRating, leaderVariance);
            FollowStats = new RoleStats(followerRating, followerVariance);
        }
    }
}
