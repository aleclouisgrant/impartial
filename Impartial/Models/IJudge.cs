using System;
using System.Collections.Generic;

namespace Impartial
{
    public interface IJudge : IUser
    {
        public Guid JudgeId { get; set; }
        public List<IFinalScore> Scores { get; set; }

        public double Accuracy { get; }
        public double Top5Accuracy { get; }
    }    
}
