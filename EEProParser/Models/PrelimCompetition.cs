using System;
using System.Collections.Generic;
using System.Linq;

namespace Impartial
{
    public class PrelimCompetition
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public List<PrelimScore> PrelimScores { get; set; } = new List<PrelimScore>();

        public Division Division { get; set; }
        public Round Round { get; set; }
        public Role Role { get; set; }

        public List<Competitor> Competitors { get; set; } = new List<Competitor>();
        public List<Judge> Judges { get; set; } = new List<Judge>();

        public List<Competitor> PromotedCompetitors { get; set; } = new List<Competitor>();

        public PrelimCompetition(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
        }

        public PrelimCompetition(
            DateTime dateTime, 
            Division division, 
            Round round, 
            Role role, 
            IEnumerable<PrelimScore> prelimScores,
            IEnumerable<Competitor> competitors,
            IEnumerable<Judge> judges,
            IEnumerable<Competitor> promotedCompetitors,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            DateTime = dateTime;
            Division = division;
            Round = round;
            Role = role;

            PrelimScores = prelimScores.ToList();
            Competitors = competitors.ToList();
            Judges = judges.ToList();

            PromotedCompetitors = promotedCompetitors.ToList();
        }

        public string ToLongString()
        {
            string str = string.Empty;
            str += Environment.NewLine + "----------";
            
            switch (Round)
            {
                case Round.Quarterfinals:
                    str += Environment.NewLine + "QUARTERS:";
                    break;
                case Round.Semifinals:
                    str += Environment.NewLine + "SEMIS:";
                    break;
                case Round.Prelims:
                    str += Environment.NewLine + "PRELIMS:";
                    break;
            }

            switch (Role)
            {
                case Role.Leader:
                    str += Environment.NewLine + Environment.NewLine + "LEADERS (" + Competitors.Count + "):";
                    break;
                case Role.Follower:
                    str += Environment.NewLine + Environment.NewLine + "FOLLOWERS (" + Competitors.Count + "):";
                    break;
            }

            str += Environment.NewLine + "JUDGES: ";
            foreach (Judge judge in Judges)
            {
                str += judge.ToString() + ", ";
            }
            str = str.Remove(str.Length - 2, 2);

            foreach (var competitor in Competitors)
            {
                List<PrelimScore> scores = PrelimScores.Where(s => s.Competitor == competitor).ToList();

                str += Environment.NewLine + competitor.FullName + ": ";
                foreach (var score in scores)
                {
                    switch (score.CallbackScore)
                    {
                        case Enums.CallbackScore.Alt1:
                            str += "A1";
                            break;
                        case Enums.CallbackScore.Alt2:
                            str += "A2";
                            break;
                        case Enums.CallbackScore.Alt3:
                            str += "A3";
                            break;
                        case Enums.CallbackScore.Yes:
                            str += " Y";
                            break;
                        default:
                        case Enums.CallbackScore.No:
                            str += " N";
                            break;
                    }
                    str += " ";
                }
            }
            return str;
        }
    }
}
