﻿using System;
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

        public static List<Competitor> CalculatePrelimRatings(List<PrelimScore> prelimScores, Role role)
        {
            var competitors = new List<Competitor>();
            var c = new List<Tuple<Competitor, List<PrelimScore>>>();

            foreach (var score in prelimScores)
            {
                if (!competitors.Any(s => s.Id == score.Competitor.Id)) { }
                    competitors.Add(score.Competitor);
            }
            foreach (var competitor in competitors)
            {
                c.Add(new Tuple<Competitor, List<PrelimScore>>(competitor, prelimScores.Where(s => s.Competitor.Id == competitor.Id).ToList()));
            }

            foreach (var competitorA in c)
            {
                double expectedScore = 0;

                foreach (var competitorB in c)
                {
                    if (competitorA != competitorB)
                    {
                        if (role == Role.Leader)
                            expectedScore = expectedScore + ExpectedScore(competitorA.Item1.LeadStats.Rating, competitorB.Item1.LeadStats.Rating);
                        else if (role == Role.Follower)
                            expectedScore = expectedScore + ExpectedScore(competitorA.Item1.FollowStats.Rating, competitorB.Item1.FollowStats.Rating);
                    }
                }

                int ratingDifference = UpdateRating(competitorA.Item1.LeadStats.Rating, competitorA.Item2.Sum(s => (int)s.CallbackScore), expectedScore) - competitorA.Item1.LeadStats.Rating;

                if (role == Role.Leader)
                    competitorA.Item1.LeadStats.Rating = competitorA.Item1.LeadStats.Rating + ratingDifference;
                else if (role == Role.Follower)
                    competitorA.Item1.FollowStats.Rating = competitorA.Item1.FollowStats.Rating + ratingDifference;
            }

            return competitors;
        }

        public static List<Couple> CalculateRatings(List<Couple> couples)
        {
            //first put the couples in order of average ranking 
            List<Couple> couplesPlaced = couples.OrderBy(o => o.ActualPlacement).ToList();

            int i = couplesPlaced.Count() + 1;
            //then compare the probability that their actual placement is their expected placement
            foreach (Couple coupleA in couplesPlaced)
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