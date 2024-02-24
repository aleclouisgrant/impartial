using Impartial;
using System;

namespace ImpartialUI
{
    public class FinalScore : IFinalScore
    {
        public Guid Id { get; set; }
        public IFinalCompetition FinalCompetition { get; set; }
        public IJudge Judge { get; set; }
        public ICompetitor Leader { get; set; }
        public ICompetitor Follower { get; set; }

        public int Placement { get; set; }
        public int ActualPlacement { get; set; }

        public double Accuracy => Util.GetAccuracy(Placement, ActualPlacement);

        public FinalScore(IFinalCompetition competition, IJudge judge, ICompetitor leader, ICompetitor follower, int placement, int actualPlacement, Guid? id = null)
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
