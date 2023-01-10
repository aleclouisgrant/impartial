using System;
using System.Collections.Generic;
using System.IO;

namespace Impartial.Services.ScoresheetParser
{
    public class WorldDanceRegistryParser : IScoresheetParser
    {
        private string _finalsSheetDoc { get; set; }

        public List<Judge> Judges { get; set; }
        public List<Score> Scores { get; set; }

        public WorldDanceRegistryParser(string finalsPath)
        {
            //if (prelimsSheetPath == null || prelimsSheetPath == String.Empty)
            //    return;
            if (finalsPath == null || finalsPath == String.Empty)
                return;

            //_prelimsSheetDoc = File.ReadAllText(prelimsSheetPath).Replace("\n", "").Replace("\r", "");
            _finalsSheetDoc = File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "");
        }

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
