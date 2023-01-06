using System;
using System.Collections.Generic;
using System.Linq;

namespace Impartial
{
    public class EloRatingService
    {
        const int k1 = 1000; //scales the probability that A wins over B at the same difference in rating. 
                             //example: k = 300 sets +200 points to be a 75% win rate
        const int k2 = 10; //scales the magnitude of the amount of points won/lost

        public static double ExpectedScore(int ratingA, int ratingB)
        {
            return 1 / (1 + Math.Pow(10, (ratingB - ratingA) / (double)k1));
        }

        public static int UpdateRating(int rating, double score, double expectedScore)
        {
            return (int)Math.Round(rating + k2 * (score - expectedScore));
        }

        public static void CalculatePrelimRatings(List<Competitor> competitors, Role role)
        {

        }

        public static List<Couple> CalculateRatings(List<Couple> couples)
        {
            //first put the couples in order of average ranking 
            List<Couple> couples_placed = couples.OrderBy(o => o.ActualPlacement).ToList();

            int i = couples_placed.Count() + 1;
            //then compare the probability that their actual placement is their expected placement
            foreach (Couple coupleA in couples_placed)
            {
                double expectedScore = 0;

                //sum up the probability of winning against each couple
                foreach (Couple coupleB in couples)
                {
                    if (coupleA != coupleB)
                    {
                        expectedScore = expectedScore + ExpectedScore(coupleA.CombinedRating, coupleB.CombinedRating);
                    }
                }

                int ratingDifference = UpdateRating(coupleA.CombinedRating, i, expectedScore) - coupleA.CombinedRating;

                string ratingChange;
                if (ratingDifference > 0)
                    ratingChange = "+" + ratingDifference;
                else
                    ratingChange = ratingDifference.ToString();

                coupleA.Leader.LeadStats.Rating = coupleA.Leader.LeadStats.Rating + ratingDifference;
                coupleA.Follower.FollowStats.Rating = coupleA.Follower.FollowStats.Rating + ratingDifference;

                Console.WriteLine("{0}. {1} ({2} => {3}) ({4}) & {5} ({6} => {7}) ({8})", coupleA.ActualPlacement,
                        coupleA.Leader.FullName, coupleA.Leader.LeadStats.Rating - ratingDifference, coupleA.Leader.LeadStats.Rating, ratingChange,
                        coupleA.Follower.FullName, coupleA.Follower.FollowStats.Rating - ratingDifference, coupleA.Follower.FollowStats.Rating, ratingChange);
                Console.WriteLine("expected score: {0}  actual score: {1}", expectedScore, i);
                Console.WriteLine("Combined Rating: {0}", coupleA.CombinedRating);

                i--;
            }

            return couples;
        }
    }
}