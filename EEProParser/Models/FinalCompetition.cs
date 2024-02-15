using System;
using System.Collections.Generic;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace Impartial
{
    public class FinalCompetition
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public List<FinalScore> FinalScores { get; set; } = new List<FinalScore>();

        public Division Division { get; set; }

        public List<Competitor> Leaders { get; set; } = new List<Competitor>();
        public List<Competitor> Followers { get; set; } = new List<Competitor>();
        public List<Judge> Judges { get; set; } = new List<Judge>();

        public FinalCompetition(Guid? id = null)
        {
            if (id == null)
                Id = Guid.NewGuid();
            else
                Id = (Guid)id;
        }

        public List<Couple> Couples => GetCouples();

        private List<Couple> GetCouples()
        {
            var couples = new List<Couple>();

            foreach (FinalScore score in FinalScores)
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
            foreach (Judge judge in Judges)
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
