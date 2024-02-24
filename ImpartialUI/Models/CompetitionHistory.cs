using System;

namespace ImpartialUI.Models
{
    public class CompetitionHistory
    {

        public string CompetitionName { get; set; }
        public DateTime CompetitionDate { get; set; }
        public int RatingBefore { get; set; }
        public int RatingAfter { get; set; }
        public int Round { get; set; }

        public int Placement { get; set; }
        public int TotalCompetitors { get; set; }

        public int RatingChange => RatingAfter - RatingBefore;

        public string RatingChangeText
        {
            get
            {
                if (RatingChange < 0)
                {
                    return RatingChange.ToString();
                }
                else
                {
                    return "+" + RatingChange.ToString();
                }
            }
        }

        public CompetitionHistory(string competitionName, DateTime competitionDate, int ratingBefore, int ratingAfter, int round, int placement, int totalCompetitors)
        {
            CompetitionName = competitionName;
            CompetitionDate = competitionDate;
            RatingBefore = ratingBefore;
            RatingAfter = ratingAfter;
            Round = round;
            Placement = placement;
            TotalCompetitors = totalCompetitors;
        }
    }
}
