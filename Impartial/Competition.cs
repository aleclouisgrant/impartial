using System;
using System.Collections.Generic;
using System.Text;

namespace Impartial
{
    public enum Role
    {
        None,
        Leader,
        Follower
    }

    public enum Division
    {
        Open,
        Newcomer,
        Novice,
        Intermediate,
        Advanced,
        AllStar,
        Champion,
    }

    public class SdcEvent
    {
        public string Name { get; }
        public DateTime Date { get; }
        public List<Competition> Competitions { get; set; }

        public SdcEvent(string name, DateTime date)
        {
            Name = name; Date = date;
            Competitions = new List<Competition>();
        }
    }

    public class Competition
    {
        public Division Division { get; }

        public List<Score> Scores { get; set; }

        public int TotalCouples => Scores?.Count ?? 0;

        public Competition(Division division)
        {
            Division = division;
        }
    }

    public class Score
    {
        public Judge Judge { get; }
        public Competitor Leader { get; set; }
        public Competitor Follower { get; set; }

        public int Placement { get; }
        public int ActualPlacement { get; }

        public double Accuracy { get; set; }

        public Score(Judge judge, Competitor leader, Competitor follower, int placement, int actualPlacement)
        {
            Judge = judge; Leader = leader; Follower = follower;
            Placement = placement; ActualPlacement = actualPlacement;
        }

        public Score(Judge judge, int placement, int actualPlacement)
        {
            Judge = judge; Placement = placement; ActualPlacement = actualPlacement;
        }
    }
}
