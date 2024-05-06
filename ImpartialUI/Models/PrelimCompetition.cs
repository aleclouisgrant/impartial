using Impartial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpartialUI.Models
{
    public class PrelimCompetition : IPrelimCompetition
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }

        public List<IPrelimScore> PrelimScores { get; set; } = new();

        public Division Division { get; set; }
        public Round Round { get; set; }
        public Role Role { get; set; }

        public List<ICompetitor> Competitors => GetCompetitors();
        public List<IJudge> Judges => GetJudges();

        public List<ICompetitor> PromotedCompetitors { get; set; } = new();

        public List<ICompetitorRegistration> CompetitorRegistrations { get; set; } = new();

        public ICompetitor Alternate1 { get; set; }
        public ICompetitor Alternate2 { get; set; }

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
            IEnumerable<ICompetitor> promotedCompetitors,
            ICompetitor alternate1,
            ICompetitor alternate2,
            IEnumerable<ICompetitorRegistration> competitorRegistrations,
            Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();

            DateTime = dateTime;
            Division = division;
            Round = round;
            Role = role;

            Alternate1 = alternate1;
            Alternate2 = alternate2;

            PrelimScores = prelimScores?.ToList();

            PromotedCompetitors = promotedCompetitors?.ToList();
            CompetitorRegistrations = competitorRegistrations?.ToList();
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

        private List<ICompetitor> GetCompetitors()
        {
            var competitors = new List<ICompetitor>();

            foreach (var prelimScore in PrelimScores)
            {
                if (prelimScore.Competitor != null && !competitors.Contains(prelimScore.Competitor))
                {
                    competitors.Add(prelimScore.Competitor);
                }
            }

            return competitors;
        }

        private List<IJudge> GetJudges()
        {
            var judges = new List<IJudge>();

            foreach (var prelimScore in PrelimScores)
            {
                if (prelimScore.Judge != null && !judges.Contains(prelimScore.Judge))
                {
                    judges.Add(prelimScore.Judge);
                }
            }

            return judges;
        }

        public void Clear()
        {
            PrelimScores.Clear();
        }
    }
}
