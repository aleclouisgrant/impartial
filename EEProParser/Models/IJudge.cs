using System.Collections.Generic;

namespace Impartial
{
    public interface IJudge : IPersonModel
    {
        public List<IFinalScore> Scores { get; set; }

        public double Accuracy { get; }
        public double Top5Accuracy { get; }
    }    
}
