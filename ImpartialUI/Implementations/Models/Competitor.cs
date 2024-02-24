using Impartial;
using System;

namespace ImpartialUI
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
            this.Id = Guid.NewGuid();

            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public Competitor(string firstName, string lastName, int wsdcId)
        {
            this.Id = Guid.NewGuid();

            this.FirstName = firstName;
            this.LastName = lastName;
            this.WsdcId = wsdcId;
        }

        public Competitor(Guid id, int wsdcId, string firstName, string lastName, int leaderRating, double leaderVariance, int followerRating, double followerVariance)
        {
            this.Id = id;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.WsdcId = wsdcId;

            LeadStats = new RoleStats(leaderRating, leaderVariance);
            FollowStats = new RoleStats(followerRating, followerVariance);
        }
    }
}
