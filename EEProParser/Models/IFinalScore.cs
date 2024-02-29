using System;

namespace Impartial
{
    public interface IFinalScore
    {
        public Guid Id { get; set; }
        public IJudge Judge { get; set; }
        public ICompetitor Leader { get; set; }
        public ICompetitor Follower { get; set; }

        public int Score { get; set; }
        public int Placement { get; set; }

        public double Accuracy { get; }

        public void SetJudge(Guid? id);
        public void SetLeader(Guid? id);
        public void SetFollower(Guid? id);
    }
}
