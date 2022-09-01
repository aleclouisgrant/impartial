using System;
using System.Collections.Generic;
using System.Linq;

namespace Impartial
{
    public class Competition
    {
        public Guid Id { get; }

        public Division Division { get; }

        public List<Score> Scores { get; set; }

        public int TotalCouples => Couples?.Count ?? 0;

        public List<Couple> Couples => GetCouples();

        public List<Judge> Judges => GetJudges();

        public Competition() { }

        public Competition(Division division)
        {
            Division = division;
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

        private List<Couple> GetCouples()
        {
            var couples = new List<Couple>();

            foreach (Score score in Scores)
            {
                if (!couples.Any(c => c.ActualPlacement == score.ActualPlacement))
                {
                    var couple = new Couple()
                    {
                        Leader = score.Leader,
                        Follower = score.Follower,
                        ActualPlacement = score.ActualPlacement,
                    };

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
    }
}
