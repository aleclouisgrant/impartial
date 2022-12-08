using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Impartial
{
    public class RoleStats
    {
        private int _numberOfCompetitions;
        public int NumberOfCompetitions { get { return _numberOfCompetitions; } }

        public List<int> Placements = new List<int>();

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
        public double Variance { get { return _rating; } }


        public RoleStats(int rating = 1000, int variance = 500)
        {
            this._rating = rating;
            _numberOfCompetitions = 0;
        }

        private void UpdateRating(int newRating)
        {
            int oldRating = _rating;
            _rating = newRating;

            //update variance
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
            this.FirstName = firstName;
            this.LastName = lastName;
        }

        public Competitor(string firstName, string lastName, int wsdcId)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.WsdcId = wsdcId;
        }
    }
}
