using Impartial;
using Impartial.Enums;
using System;
using System.Linq;

namespace ImpartialUI.Models
{
    public class PrelimScore : IPrelimScore
    {
        private Guid _judgeId;
        private Guid _competitorId;

        public Guid Id { get; }
        public IPrelimCompetition PrelimCompetition { get; set; }
        public IJudge Judge { get; set; }
        public ICompetitor Competitor { get; set; }
        public CallbackScore CallbackScore { get; set; }

        public PrelimScore(IPrelimCompetition prelimCompetition, Guid judgeId, Guid competitorId, CallbackScore callbackScore, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            PrelimCompetition = prelimCompetition;
            SetJudge(judgeId);
            SetCompetitor(competitorId);

            CallbackScore = callbackScore;
        }

        public void SetJudge(Guid id)
        {
            _judgeId = id;
            Judge = App.JudgesDb.Where(j => j.Id == _judgeId).FirstOrDefault();
        }

        public void SetCompetitor(Guid id)
        {
            _competitorId = id;
            Competitor = App.CompetitorsDb.Where(j => j.Id == _competitorId).FirstOrDefault();
        }
    }
}
