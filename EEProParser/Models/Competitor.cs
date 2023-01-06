using Impartial.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Impartial
{
    public class RoleStats
    {
        private int _rating;
        public int Rating
        {
            get { return _rating; }
            set { UpdateRating(value); }
        }

        public int AdjustedRating
        {
            get { return _rating - (int)Math.Ceiling(3 * Variance); }
        }

        private double _variance;
        public double Variance { get { return _variance; } }

        public RoleStats(int rating = 1000, double variance = 500)
        {
            _rating = rating;
            _variance = variance;
        }

        private void UpdateRating(int newRating)
        {
            int oldRating = _rating;
            _rating = newRating;

            //TODO: update variance
            if (newRating > oldRating)
            {

            }
        }
    }

    public class Competitor : PersonModel
    {
        // personal info
        public string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;

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
