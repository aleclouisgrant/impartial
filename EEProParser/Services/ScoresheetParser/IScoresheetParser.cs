using Impartial;
using System;
using System.Collections.Generic;
using System.Text;

namespace Impartial
{
    public interface IScoresheetParser
    {
        List<Judge> Judges { get; set; }
        List<Score> Scores { get; set; }

        public List<Division> GetDivisions();
        public List<Judge> GetJudgesByDivision(Division division);
        public Competition GetCompetition(Division division);
    }
}
