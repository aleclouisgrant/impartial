using Impartial.Enums;
using System;

namespace Impartial
{
    public interface IPrelimScore
    {
        public Guid Id { get; }
        public IPrelimCompetition PrelimCompetition { get; }
        public IJudge Judge { get; }
        public ICompetitor Competitor { get; }

        public Role Role { get; set; }
        public bool Finaled { get; set; }
        public CallbackScore CallbackScore { get; set; }
    }
}
