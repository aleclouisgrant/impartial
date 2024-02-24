using System;

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

    public interface ICompetitor : IPersonModel
    {
        // personal info
        public new string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;

        public int WsdcId { get; set; }

        public RoleStats LeadStats { get; set; }
        public RoleStats FollowStats { get; set; }
    }
}
