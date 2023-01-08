using iText.Barcodes.Dmcode;
using System;

namespace Impartial
{
    public class Score
    {
        public Guid Id { get; set; }
        public Competition Competition { get; set; }
        public Judge Judge { get; set; }
        public Competitor Leader { get; set; }
        public Competitor Follower { get; set; }

        public int Placement { get; set; }
        public int ActualPlacement { get; set; }

        public double Accuracy => Util.GetAccuracy(Placement, ActualPlacement);

        public Score(Competition competition, Judge judge, Competitor leader, Competitor follower, int placement, int actualPlacement)
        {
            Id = Guid.NewGuid();

            Competition = competition;
            Judge = judge; Leader = leader; Follower = follower;
            Placement = placement; ActualPlacement = actualPlacement;
        }

        public Score(Judge judge, int placement, int actualPlacement)
        {
            Id = Guid.NewGuid(); 
            
            Judge = judge; 
            Placement = placement; ActualPlacement = actualPlacement;
        }

        public Score(Guid id, Competition competition, Judge judge, Competitor leader, Competitor follower, int placement, int actualPlacement)
        { 
            Id = id; 
            Competition = competition;
            Judge = judge;
            Leader = leader;
            Follower = follower;
            Placement = placement;
            ActualPlacement = actualPlacement;
        }
    }
}
