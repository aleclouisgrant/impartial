using System;

namespace Impartial
{
    public class FinalScore
    {
        public Guid Id { get; set; }
        public FinalCompetition FinalCompetition { get; set; }
        public Judge Judge { get; set; }
        public Competitor Leader { get; set; }
        public Competitor Follower { get; set; }

        public int Placement { get; set; }
        public int ActualPlacement { get; set; }

        public double Accuracy => Util.GetAccuracy(Placement, ActualPlacement);

        public FinalScore(FinalCompetition competition, Judge judge, Competitor leader, Competitor follower, int placement, int actualPlacement, Guid? id = null)
        {
            if (id == null)
                Id = Guid.NewGuid();
            else
                Id = (Guid)id;

            FinalCompetition = competition;
            Judge = judge; Leader = leader; Follower = follower;
            Placement = placement; ActualPlacement = actualPlacement;
        }
    }
}
