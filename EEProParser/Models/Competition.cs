using System;
using System.Collections.Generic;
using System.Linq;

namespace Impartial
{
    public class Competition
    {
        public Guid Id { get; }

        public string Name { get; set; }

        public Division Division { get; set; }

        public List<Score> Scores { get; set; } = new List<Score>();

        public int TotalCouples => Couples?.Count ?? 0;

        public List<Couple> Couples { get; set; } = new List<Couple>(); //temporary until GetCouples works

        public List<Judge> Judges => GetJudges();

        public Competition() 
        { 
            Id = Guid.NewGuid();
        }

        public Competition(string name)
        {
            Name = name;

            Id = Guid.NewGuid();
        }

        public Competition(Division division)
        {
            Division = division;

            Id = Guid.NewGuid();
        }

        public Competition(string name, Division division)
        {
            Name = name;
            Division = division;

            Id = Guid.NewGuid();
        }

        private List<Judge> GetJudges()
        {
            var judges = new List<Judge>();

            foreach (var score in Scores)
            {
                if (!judges.Contains(score.Judge))
                    judges.Add(score.Judge);
            }

            return judges;
        }

        // this is good but needs more built around it to work as intended
        private List<Couple> GetCouples()
        {
            var couples = new List<Couple>();

            foreach (Score score in Scores)
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

        public override string ToString()
        {
            string str = "COMPETITION (" + Division + ", " + TotalCouples + " couples)";

            for (int placement = 1; placement <= TotalCouples; placement++)
            {
                var couple = Couples[placement - 1];

                str += System.Environment.NewLine + placement + ": " +
                    couple.Leader.FullName + " & " +
                    couple.Follower.FullName;

                var scores = new List<int>();

                foreach (var score in Scores)
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

        public void Clear()
        {
            Couples.Clear();
            Scores.Clear();
            Judges.Clear();
        }
    }
}
