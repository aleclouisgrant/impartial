using System;
using System.Collections.Generic;
using System.Text;

namespace Impartial.Services.ScoresheetParser
{
    public class WorldDanceRegistryParser : IScoresheetParser
    {
        public List<Judge> Judges { get; set; }
        public List<Score> Scores { get; set; }

        public Competition GetCompetition(Division division)
        {
            throw new NotImplementedException();
        }

        public List<Division> GetDivisions()
        {
            throw new NotImplementedException();
        }

        public List<Judge> GetJudgesByDivision(Division division)
        {
            throw new NotImplementedException();
        }
    }
}
