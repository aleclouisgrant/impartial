using System;

namespace Impartial
{
    public interface IFinalScore
    {
        public Guid Id { get; set; }
        public IFinalCompetition FinalCompetition { get; set; }
        public IJudge Judge { get; set; }
        public ICompetitor Leader { get; set; }
        public ICompetitor Follower { get; set; }

        public int Score { get; set; }
        public int Placement { get; set; }

        public double Accuracy { get; }
    }
}
