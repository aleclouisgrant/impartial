using System.Collections.Generic;

namespace Impartial
{
    public class Couple
    {
        public Competitor Leader { get; set; }
        public Competitor Follower { get; set; }
        public int ActualPlacement { get; set; }

        public List<Score> Scores { get; set; } = new List<Score>();

        public Couple(Competitor leader, Competitor follower, int placement)
        {
            Leader = leader;
            Follower = follower;
            ActualPlacement = placement;
        }
    }
}
