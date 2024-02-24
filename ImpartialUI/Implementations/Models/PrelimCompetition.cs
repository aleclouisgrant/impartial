using Impartial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpartialUI
{
    public class PrelimCompetition : IPrelimCompetition
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public List<IPrelimScore> PrelimScores { get; set; } = new();

        public Division Division { get; set; }
        public Round Round { get; set; }
        public Role Role { get; set; }

        public List<ICompetitor> Competitors { get; set; } = new();
        public List<IJudge> Judges { get; set; } = new();

        public List<ICompetitor> PromotedCompetitors { get; set; } = new();

        public PrelimCompetition(Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
        }

        public PrelimCompetition(
            DateTime dateTime,
            Division division,
            Round round,
            Role role,
            IEnumerable<IPrelimScore> prelimScores,
            IEnumerable<ICompetitor> competitors,
            IEnumerable<IJudge> judges,
            IEnumerable<ICompetitor> promotedCompetitors,
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
            foreach (IJudge judge in Judges)
            {
                str += judge.ToString() + ", ";
            }
            str = str.Remove(str.Length - 2, 2);

            foreach (var competitor in Competitors)
            {
                List<IPrelimScore> scores = PrelimScores.Where(s => s.Competitor == competitor).ToList();

                str += Environment.NewLine + competitor.FullName + ": ";
                foreach (var score in scores)
                {
                    switch (score.CallbackScore)
                    {
                        case Impartial.Enums.CallbackScore.Alt1:
                            str += "A1";
                            break;
                        case Impartial.Enums.CallbackScore.Alt2:
                            str += "A2";
                            break;
                        case Impartial.Enums.CallbackScore.Alt3:
                            str += "A3";
                            break;
                        case Impartial.Enums.CallbackScore.Yes:
                            str += " Y";
                            break;
                        default:
                        case Impartial.Enums.CallbackScore.No:
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
