using Impartial.Enums;
using System;

namespace Impartial
{
    public interface IPrelimScore
    {
        public Guid Id { get; }
        public IJudge Judge { get; set; }
        public ICompetitorRegistration CompetitorRegistration { get; set; }
        public ICompetitor Competitor { get; set; }
        public CallbackScore CallbackScore { get; set; }

        public void SetJudge(Guid? id);
        public void SetCompetitor(Guid? id);
        public bool TrySetJudge(Guid? id);
        public bool TrySetCompetitor(Guid? id);

    }
}
