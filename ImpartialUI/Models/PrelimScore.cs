using Impartial;
using Impartial.Enums;
using System;
using System.Linq;

namespace ImpartialUI.Models
{
    public class PrelimScore : IPrelimScore
    {
        private Guid? _judgeId;
        private Guid? _competitorId;

        public Guid Id { get; }
        public IJudge Judge { get; set; }
        public ICompetitor Competitor { get; set; }
        public CallbackScore CallbackScore { get; set; }

        public PrelimScore(Guid? judgeId, Guid? competitorId, CallbackScore callbackScore, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            SetJudge(judgeId);
            SetCompetitor(competitorId);

            CallbackScore = callbackScore;
        }

        public PrelimScore(IJudge judge, ICompetitor competitor, CallbackScore callbackScore, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            Judge = judge;
            Competitor = competitor;
            CallbackScore = callbackScore;
        }

        public void SetJudge(Guid? id)
        {
            _judgeId = id;
            Judge = App.JudgesDb.Where(j => j.JudgeId == _judgeId).FirstOrDefault();
        }

        public void SetCompetitor(Guid? id)
        {
            _competitorId = id;
            Competitor = App.CompetitorsDb.Where(c => c.CompetitorId == _competitorId).FirstOrDefault();
        }
    }
}
