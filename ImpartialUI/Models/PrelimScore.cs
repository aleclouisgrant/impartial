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
        public ICompetitorRegistration CompetitorRegistration { get; set; }

        public ICompetitor Competitor
        {
            get
            {
                return CompetitorRegistration?.Competitor;
            }
            set
            {
                if (CompetitorRegistration == null)
                {
                    CompetitorRegistration = new CompetitorRegistration(value, string.Empty);
                }
                else
                {
                    CompetitorRegistration.Competitor = value;
                }
            }
        }
        public CallbackScore CallbackScore { get; set; }

        public PrelimScore(IJudge judge, ICompetitorRegistration competitorRegistration, CallbackScore callbackScore, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            Judge = judge;
            CompetitorRegistration = competitorRegistration;
            CallbackScore = callbackScore;
        }

        public PrelimScore(Guid? judgeId, Guid? competitorId, CallbackScore callbackScore, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            TrySetJudge(judgeId);
            TrySetCompetitor(competitorId);

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

            if (Judge == null)
                throw new Exception("Judge ID not found in Judges DB");
        }

        public void SetCompetitor(Guid? id)
        {
            Competitor = App.CompetitorsDb.FirstOrDefault(c => c.CompetitorId == id);

            if (Competitor == null)
                throw new Exception("Competitor ID not found in Competitors DB");
        }

        public bool TrySetJudge(Guid? id)
        {
            Judge = App.JudgesDb.FirstOrDefault(j => j.JudgeId == id);

            if (Judge == null)
                return false;
            else 
                return true;
        }

        public bool TrySetCompetitor(Guid? id)
        {
            Competitor = App.CompetitorsDb.FirstOrDefault(j => j.CompetitorId == id);

            if (Competitor == null)
                return false;
            else
                return true;
        }
    }
}
