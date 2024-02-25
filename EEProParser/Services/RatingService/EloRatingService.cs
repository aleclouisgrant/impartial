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

        public static List<ICompetitor> PrelimRatings(IPrelimCompetition prelimCompetition)
        {
            List<ICompetitor> competitors = new();

            //TODO:
            //List<Tuple<ICompetitor, List<IPrelimScore>>> finalists = new(); 
            //List<Tuple<ICompetitor, List<IPrelimScore>>> nonFinalists = new();  

            //foreach (var prelimScore in prelimCompetition.PrelimScores)
            //{
            //    if (prelimCompetition.PromotedCompetitors.Contains(prelimScore.Competitor))
            //    {
            //        var competitor = finalists.Where(f => f.Item1.Id == prelimScore.Competitor.Id).FirstOrDefault();

            //        if (competitor != null)
            //        {

            //        }
            //    }
            //    else
            //    {

            //    }
            //}

            //int totalNumber = prelimCompetition.Competitors.Count();
            //int finalistSpots = finalists.Count();
            //int ratingSum = 0;
            //double maxScore = 1;

            //if (prelimCompetition.Role == Role.Leader)
            //{
            //    ratingSum = finalists.Sum(c => c.Item1.LeadStats.Rating);
            //    maxScore = prelimCompetition.Judges.Count() * 10;
            //}
            //else if (prelimCompetition.Role == Role.Follower)
            //{
            //    ratingSum = finalists.Sum(c => c.Item1.FollowStats.Rating);
            //    maxScore = prelimCompetition.Judges.Count() * 10;
            //}

            //int averageRating = ratingSum / finalistSpots;

            //foreach (var competitor in prelimCompetition.Competitors)
            //{
            //    int ratingDifference = 0;

            //    double score = prelimCompetition.PrelimScores.Where(s => s.Competitor.Id == competitor.Id).Sum(s => (int)s.CallbackScore) / 10;
            //    double normalizedScore = (score / maxScore);
            //    double bonus = ((double)(round - 1) * 0.3);
            //    bonus = 0; //taking out bonus for now

            //    if (prelimCompetition.Role == Role.Leader)
            //    {
            //        double expectedScore = ExpectedScore(competitor.LeadStats.Rating, averageRating);
            //        ratingDifference = UpdateRating(competitor.LeadStats.Rating, normalizedScore + bonus, expectedScore) - competitor.LeadStats.Rating;
            //        competitor.LeadStats.Rating = competitor.LeadStats.Rating + ratingDifference;
            //    }
            //    else if (prelimCompetition.Role == Role.Follower)
            //    {
            //        double expectedScore = ExpectedScore(competitor.FollowStats.Rating, averageRating);
            //        ratingDifference = UpdateRating(competitor.FollowStats.Rating, normalizedScore + bonus, expectedScore) - competitor.FollowStats.Rating;
            //        competitor.FollowStats.Rating = competitor.FollowStats.Rating + ratingDifference;
            //    }

            //    string ratingChange;
            //    if (ratingDifference > 0)
            //        ratingChange = "+" + ratingDifference;
            //    else
            //        ratingChange = ratingDifference.ToString();

            //    if (finalists.Any(c => c.Item1.Id == competitor.Id))
            //        Trace.Write("*");

            //    if (prelimCompetition.Role == Role.Leader)
            //        Trace.WriteLine(competitor.FullName + " (" + (competitor.LeadStats.Rating - ratingDifference).ToString() + " => " + competitor.LeadStats.Rating + ") (" + ratingChange + ")");
            //    else if (prelimCompetition.Role == Role.Follower)
            //        Trace.WriteLine(competitor.FullName + " (" + (competitor.FollowStats.Rating - ratingDifference).ToString() + " => " + competitor.FollowStats.Rating + ") (" + ratingChange + ")");
            //}

            return competitors;
        }

        public static List<ICouple> CalculateFinalsRating(List<ICouple> couples, bool straightToFinals = false)
        {
            //first put the couples in order of average ranking 
            List<ICouple> couplesPlaced = couples.OrderBy(o => o.ActualPlacement).ToList();

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
                double score = (((double)couplesPlaced.Count() - (double)coupleA.ActualPlacement + 1) / (double)couplesPlaced.Count());
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
                    coupleA.ActualPlacement + ". " +
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