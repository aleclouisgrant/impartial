using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Impartial
{
    public static class EloRatingService
    {
        const int k1 = 2000; //scales the probability that A wins over B at the same difference in rating. 
                             //example: k = 300 sets +200 points to be a 75% win rate

        const int k2 = 20; //scales the magnitude of the amount of points won/lost in prelims

        const int k3 = 20; //scales the magnitude of points won/lost in finals

        public static double ExpectedScore(int ratingA, int ratingB)
        {
            return 1 / (1 + Math.Pow(10, (ratingB - ratingA) / (double)k1));
        }

        public static int UpdateRating(int rating, double score, double expectedScore)
        {
            return (int)Math.Round(rating + k2 * (score - expectedScore));
        }

        public static int UpdateRatingFinals(int rating, double score, double expectedScore)
        {
            return (int)Math.Round(rating + k3 * (score - expectedScore));
        }

        public static List<ICompetitor> PrelimRatings(List<PrelimScore> prelimScores, Role role, List<IJudge> prelimJudges)
        {
            var competitors = new List<ICompetitor>();
            var finalists = new List<Tuple<ICompetitor, List<PrelimScore>>>();
            var notFinalists = new List<Tuple<ICompetitor, List<PrelimScore>>>();
            int round = prelimScores.FirstOrDefault().Round;

            foreach (var score in prelimScores)
            {
                if (!competitors.Any(c => c.Id == score.Competitor.Id))
                    competitors.Add(score.Competitor);
            }
            foreach (var competitor in competitors)
            {
                if (!finalists.Any(c => c.Item1.Id == competitor.Id) || !notFinalists.Any(c => c.Item1.Id == competitor.Id))
                {
                    var tuple = new Tuple<ICompetitor, List<PrelimScore>>(competitor, prelimScores.Where(s => s.Competitor.Id == competitor.Id).ToList());
                    if (tuple.Item2.First().Finaled)
                        finalists.Add(tuple);
                    else    
                        notFinalists.Add(tuple);
                }
            }

            int totalNumber = competitors.Count();
            int finalistSpots = finalists.Count();
            int ratingSum = 0;
            double maxScore = 1;

            if (role == Role.Leader)
            {
                ratingSum = finalists.Sum(c => c.Item1.LeadStats.Rating);
                maxScore = prelimJudges.Count() * 10;
            }
            else if (role == Role.Follower)
            {
                ratingSum = finalists.Sum(c => c.Item1.FollowStats.Rating);
                maxScore = prelimJudges.Count() * 10;
            }

            int averageRating = ratingSum / finalistSpots;

            foreach (var competitor in competitors)
            {
                int ratingDifference = 0;

                double score = prelimScores.Where(s => s.Competitor.Id == competitor.Id).Sum(s => (int)s.CallbackScore) / 10;
                double normalizedScore = (score / maxScore);
                double bonus = ((double)(round - 1) * 0.3);
                bonus = 0; //taking out bonus for now

                if (role == Role.Leader)
                {
                    double expectedScore = ExpectedScore(competitor.LeadStats.Rating, averageRating);
                    ratingDifference = UpdateRating(competitor.LeadStats.Rating, normalizedScore + bonus, expectedScore) - competitor.LeadStats.Rating;
                    competitor.LeadStats.Rating = competitor.LeadStats.Rating + ratingDifference;
                }
                else if (role == Role.Follower)
                {
                    double expectedScore = ExpectedScore(competitor.FollowStats.Rating, averageRating);
                    ratingDifference = UpdateRating(competitor.FollowStats.Rating, normalizedScore + bonus, expectedScore) - competitor.FollowStats.Rating;
                    competitor.FollowStats.Rating = competitor.FollowStats.Rating + ratingDifference;
                }

                string ratingChange;
                if (ratingDifference > 0)
                    ratingChange = "+" + ratingDifference;
                else
                    ratingChange = ratingDifference.ToString();

                if (finalists.Any(c => c.Item1.Id == competitor.Id))
                    Trace.Write("*");

                if (role == Role.Leader)
                    Trace.WriteLine(competitor.FullName + " (" + (competitor.LeadStats.Rating - ratingDifference).ToString() + " => " + competitor.LeadStats.Rating + ") (" + ratingChange + ")");
                else if (role == Role.Follower)
                    Trace.WriteLine(competitor.FullName + " (" + (competitor.FollowStats.Rating - ratingDifference).ToString() + " => " + competitor.FollowStats.Rating + ") (" + ratingChange + ")");
            }

            return competitors;
        }

        public static List<ICouple> CalculateFinalsRating(List<ICouple> couples, bool straightToFinals = false)
        {
            //first put the couples in order of average ranking 
            List<ICouple> couplesPlaced = couples.OrderBy(o => o.Placement).ToList();

            //then compare the probability that their actual placement is their expected placement
            foreach (ICouple coupleA in couplesPlaced)
            {
                double expectedScore = 0;

                //sum up the probability of winning against each couple
                foreach (ICouple coupleB in couples)
                {
                    if (coupleA != coupleB)
                    {
                        expectedScore = expectedScore + ExpectedScore(coupleA.CombinedRating, coupleB.CombinedRating);
                    }
                }

                expectedScore = (expectedScore / couplesPlaced.Count());
                double score = (((double)couplesPlaced.Count() - (double)coupleA.Placement + 1) / (double)couplesPlaced.Count());
                if (!straightToFinals)
                    score = score + 0.2; // BONUS

                int ratingDifference = UpdateRatingFinals(coupleA.CombinedRating, score, expectedScore) - coupleA.CombinedRating;

                string ratingChange;
                if (ratingDifference > 0)
                    ratingChange = "+" + ratingDifference;
                else
                    ratingChange = ratingDifference.ToString();

                coupleA.Leader.LeadStats.Rating = coupleA.Leader.LeadStats.Rating + ratingDifference;
                coupleA.Follower.FollowStats.Rating = coupleA.Follower.FollowStats.Rating + ratingDifference;

                Trace.WriteLine("{0}. {1} ({2} => {3}) ({4}) & {5} ({6} => {7}) ({8})",
                    coupleA.Placement + ". " +
                    coupleA.Leader.FullName + " (" + (coupleA.Leader.LeadStats.Rating - ratingDifference).ToString() + " => " + 
                    coupleA.Leader.LeadStats.Rating + ") (" + ratingChange + ") & " + 
                    coupleA.Follower.FullName + " (" + (coupleA.Follower.FollowStats.Rating - ratingDifference).ToString() + " => " + 
                    coupleA.Follower.FollowStats.Rating + " ) (" +  ratingChange + ")");
                Trace.WriteLine("Expected Score: " + expectedScore + "  Actual Score: " + score);
                Trace.WriteLine("Combined Rating: " + coupleA.CombinedRating);
            }

            return couples;
        }
    }
}