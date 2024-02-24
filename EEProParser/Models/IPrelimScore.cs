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
        public CallbackScore CallbackScore { get; set; }
    }
}
