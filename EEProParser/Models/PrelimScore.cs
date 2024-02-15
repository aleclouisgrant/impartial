using Impartial.Enums;
using System;

namespace Impartial
{
    public class PrelimScore
    {
        public Guid Id { get; }
        public PrelimCompetition PrelimCompetition { get; set; }
        public Judge Judge { get; set; } 
        public Competitor Competitor { get; set; }
        
        public Role Role { get; set; }
        public bool Finaled { get; set; }
        public CallbackScore CallbackScore { get; set; }

        public PrelimScore(PrelimCompetition competition, Judge judge, Competitor competitor, Role role, bool finaled, CallbackScore callbackScore, int rawScore, int round, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            PrelimCompetition = competition;
            Judge = judge;
            Competitor = competitor;

            Role = role;
            Finaled = finaled;
            CallbackScore = callbackScore;
        }
    }
}
