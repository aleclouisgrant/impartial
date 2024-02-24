using Impartial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpartialUI
{
    public class FinalCompetition : IFinalCompetition
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public List<IFinalScore> FinalScores { get; set; } = new List<IFinalScore>();

        public Division Division { get; set; }

        public List<ICompetitor> Leaders { get; set; } = new();
        public List<ICompetitor> Followers { get; set; } = new();
        public List<IJudge> Judges { get; set; } = new List<IJudge>();

        public FinalCompetition(Guid? id = null)
        {
            if (id == null)
                Id = Guid.NewGuid();
            else
                Id = (Guid)id;
        }

        public List<ICouple> Couples => GetCouples();

        private List<ICouple> GetCouples()
        {
            var couples = new List<ICouple>();

            foreach (IFinalScore score in FinalScores)
            {
                if (!couples.Any(c => c.ActualPlacement == score.ActualPlacement))
                {
                    var couple = new Couple(score.Leader, score.Follower, score.ActualPlacement);
                    couples.Add(couple);
                    couple.Scores.Add(score);
                }
                else
                {
                    var couple = couples.Find(c => c.ActualPlacement == score.ActualPlacement);
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
                        scores.Add(score.Placement);
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
