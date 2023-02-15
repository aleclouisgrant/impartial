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

        public List<Score> Scores { get; set; } = new List<Score>();

        public List<PrelimScore> LeaderPrelimScores { get; set; } = new List<PrelimScore>();
        public List<PrelimScore> FollowerPrelimScores { get; set; } = new List<PrelimScore>();

        public int TotalCouples => Couples?.Count ?? 0;
        public int TotalLeaders
        {
            get
            {
                try
                {
                    return LeaderPrelimScores?.Count() / PrelimLeaderJudges?.Count() ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }
        public int TotalFollowers
        {
            get
            {
                try
                {
                    return FollowerPrelimScores?.Count() / PrelimFollowerJudges?.Count() ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public List<Couple> Couples => GetCouples();
        public List<Judge> Judges => GetJudges();
        public List<Judge> PrelimLeaderJudges => GetPrelimJudges(Role.Leader);
        public List<Judge> PrelimFollowerJudges => GetPrelimJudges(Role.Follower);
        public List<Competitor> PrelimLeaders => GetPrelimCompetitors(Role.Leader);
        public List<Competitor> FinalLeaders => GetFinalCompetitors(Role.Leader);
        public List<Competitor> PrelimFollowers => GetPrelimCompetitors(Role.Follower);
        public List<Competitor> FinalFollowers => GetFinalCompetitors(Role.Follower);

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
        private List<Judge> GetPrelimJudges(Role role)
        {
            var judges = new List<Judge>();

            if (role == Role.Leader)
            {
                foreach (var score in LeaderPrelimScores)
                {
                    if (!judges.Any(j => j.Id == score.Judge.Id))
                        judges.Add(score.Judge);
                }
            }
            else if (role == Role.Follower)
            {
                foreach (var score in FollowerPrelimScores)
                {
                    if (!judges.Any(j => j.Id == score.Judge.Id))
                        judges.Add(score.Judge);
                }
            }

            return judges;
        }
        private List<Competitor> GetPrelimCompetitors(Role role)
        {
            var competitors = new List<Competitor>();

            if (role == Role.Leader)
            {
                foreach (var score in LeaderPrelimScores)
                {
                    if (!competitors.Any(s => s.Id == score.Competitor.Id))
                        competitors.Add(score.Competitor);
                }
            }
            else if (role == Role.Follower)
            {
                foreach (var score in FollowerPrelimScores)
                {
                    if (!competitors.Any(s => s.Id == score.Competitor.Id))
                        competitors.Add(score.Competitor);
                }
            }

            return competitors;
        }
        private List<Competitor> GetFinalCompetitors(Role role)
        {
            var competitors = new List<Competitor>();

            foreach (Score score in Scores)
            {
                if (role == Role.Leader)
                {
                    if (!competitors.Any(c => c.Id == score.Leader.Id))
                        competitors.Add(score.Leader);
                }
                else if (role == Role.Follower)
                {
                    if (!competitors.Any(c => c.Id == score.Follower.Id))
                        competitors.Add(score.Follower);
                }
            }

            return competitors;
        }
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

            str += System.Environment.NewLine + "----------";
            str += System.Environment.NewLine + "PRELIMS:";
            str += System.Environment.NewLine + System.Environment.NewLine + "LEADERS:";
            str += System.Environment.NewLine + "JUDGES: ";
            foreach (Judge judge in PrelimLeaderJudges)
            {
                str += judge.ToString() + ", ";
            }
            str = str.Remove(str.Length - 2, 2);

            List<Competitor> leads = GetPrelimCompetitors(Role.Leader);
            foreach (var lead in leads)
            {
                List<PrelimScore> scores = LeaderPrelimScores.Where(s => s.Competitor == lead).ToList();

                str += System.Environment.NewLine + lead.FullName + ": ";
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

            str += System.Environment.NewLine + System.Environment.NewLine + "FOLLOWERS:";
            str += System.Environment.NewLine + "JUDGES: ";
            foreach (Judge judge in PrelimFollowerJudges)
            {
                str += judge.ToString() + ", ";
            }
            str = str.Remove(str.Length - 2, 2);

            List<Competitor> follows = GetPrelimCompetitors(Role.Follower);
            foreach (var follow in follows)
            {
                List<PrelimScore> scores = FollowerPrelimScores.Where(s => s.Competitor == follow).ToList();

                str += System.Environment.NewLine + follow.FullName + ": ";
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

        public void Clear()
        {
            Scores.Clear();
            LeaderPrelimScores.Clear();
            FollowerPrelimScores.Clear();
        }
        public void ClearFinals()
        {
            Scores.Clear();
        }
        public void ClearPrelims()
        {
            LeaderPrelimScores.Clear();
            FollowerPrelimScores.Clear();
        }
    }
}
