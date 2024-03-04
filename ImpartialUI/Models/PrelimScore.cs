using Impartial;
using Impartial.Enums;
using System;
using System.Linq;

namespace ImpartialUI.Models
{
    public class PrelimScore : IPrelimScore
    {
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
            Judge = App.JudgesDb.FirstOrDefault(j => j.JudgeId == id);
        }

        public void SetCompetitor(Guid? id)
        {
            Competitor = App.CompetitorsDb.FirstOrDefault(c => c.CompetitorId == id);
        }
    }
}
