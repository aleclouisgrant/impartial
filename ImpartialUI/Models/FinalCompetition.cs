using Impartial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpartialUI.Models
{
    public class FinalCompetition : IFinalCompetition
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public List<IFinalScore> FinalScores { get; set; } = new List<IFinalScore>();

        public Division Division { get; set; }

        public List<ICompetitor> Leaders => GetLeaders();
        public List<ICompetitor> Followers => GetFollowers();
        public List<IJudge> Judges => GetJudges();

        public List<ICouple> Couples => GetCouples();

        public FinalCompetition(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
        }

        public FinalCompetition(DateTime dateTime, Division division, List<IFinalScore> finalScores, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
        }

        private List<ICompetitor> GetLeaders()
        {
            var competitors = new List<ICompetitor>();

            foreach (var finalScore in FinalScores)
            {
                if (finalScore.Leader != null && !competitors.Contains(finalScore.Leader))
                {
                    competitors.Add(finalScore.Leader);
                }
            }

            return competitors;
        }

        private List<ICompetitor> GetFollowers()
        {
            var competitors = new List<ICompetitor>();

            foreach (var finalScore in FinalScores)
            {
                if (finalScore.Follower != null && !competitors.Contains(finalScore.Follower))
                {
                    competitors.Add(finalScore.Follower);
                }
            }

            return competitors;
        }

        private List<IJudge> GetJudges()
        {
            var judges = new List<IJudge>();

            foreach (var finalScore in FinalScores)
            {
                if (finalScore.Judge != null && !judges.Contains(finalScore.Judge))
                {
                    judges.Add(finalScore.Judge);
                }
            }

            return judges;
        }

        private List<ICouple> GetCouples()
        {
            var couples = new List<ICouple>();

            foreach (IFinalScore score in FinalScores)
            {
                if (!couples.Any(c => c.Placement == score.Placement))
                {
                    var couple = new Couple(score.Leader, score.Follower, score.Placement);
                    couples.Add(couple);
                    couple.Scores.Add(score);
                }
                else
                {
                    var couple = couples.Find(c => c.Placement == score.Placement);
                    couple.Scores.Add(score);
                }
            }

            return couples;
        }

        public string ToLongString()
        {
            string str = string.Empty;
            str += Environment.NewLine + "JUDGES: ";
            foreach (IJudge judge in Judges)
            {
                str += judge.ToString() + ", ";
            }
            str = str.Remove(str.Length - 2, 2);

            str += Environment.NewLine + "PLACEMENTS:";

            for (int placement = 1; placement <= Couples.Count; placement++)
            {
                var couple = Couples[placement - 1];

                if (couple.Leader is null || couple.Follower is null)
                    return str;

                str += Environment.NewLine + placement + ": " +
                    couple.Leader.FullName + " & " +
                    couple.Follower.FullName;
                var scores = new List<int>();

                foreach (var score in FinalScores)
                {
                    if (score.Leader == couple.Leader && score.Follower == couple.Follower)
                    {
                        scores.Add(score.Score);
                    }
                }

                if (scores.Count > 0)
                {
                    str += " (";

                    scores = scores.OrderBy(s => s).ToList();

                    for (int i = 0; i < scores.Count; i++)
                    {
                        str += scores[i] + " ";
                    }

                    str = str.Remove(str.Length - 1);
                    str += ")";
                }
            }

            return str;
        }
    }
}
