namespace Impartial
{
    public class Score
    {
        public Judge Judge { get; }
        public Competitor Leader { get; set; }
        public Competitor Follower { get; set; }

        public int Placement { get; set; }
        public int ActualPlacement { get; set; }

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
