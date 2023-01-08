using System;
using System.Collections.Generic;
using System.Linq;

namespace Impartial
{
    public class Competition
    {
        public Guid Id { get; }

        public string Name { get; set; }

        public DateTime Date { get; set; }

        public Division Division { get; set; }

        private List<Score> _scores = new List<Score>();
        public List<Score> Scores 
        {
            get { return _scores; }
            set
            {
                _scores = value;
            } 
        }

        public int TotalCouples => Couples?.Count ?? 0;

        public List<Couple> Couples => GetCouples(); //temporary until GetCouples works

        public List<Judge> Judges => GetJudges();

        public Competition() 
        { 
            Id = Guid.NewGuid();
        }

        public Competition(string name, DateTime date)
        {
            Id = Guid.NewGuid();
            
            Name = name;
            Date = date;
        }

        public Competition(Division division)
        {
            Id = Guid.NewGuid();

            Division = division;
        }

        public Competition(string name, Division division)
        {
            Id = Guid.NewGuid();

            Name = name;
            Division = division;
        }

        public Competition(Guid id, string name, DateTime date, Division division)
        {
            Id = id;
            Name = name;
            Date = date;
            Division = division;
        }

        private List<Judge> GetJudges()
        {
            var judges = new List<Judge>();

            foreach (var score in Scores)
            {
                if (!judges.Any(j => j.Id == score.Judge.Id))
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
            return Name + " " + Division + " Jack & Jill (" + Date.ToShortDateString() + ")";
        }

        public string ToLongString()
        {
            string str = Name + " " + Division + " Jack & Jill (" + Date.ToShortDateString() + ", " + TotalCouples + " couples)";

            str += System.Environment.NewLine + "JUDGES: ";
            foreach (Judge judge in Judges)
            {
                str += judge.ToString() + ", ";
            }
            str = str.Remove(str.Length - 2, 2);

            str += System.Environment.NewLine + "PLACEMENTS:";

            for (int placement = 1; placement <= TotalCouples; placement++)
            {
                var couple = Couples[placement - 1];

                if (couple.Leader is null || couple.Follower is null)
                    return str;

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
