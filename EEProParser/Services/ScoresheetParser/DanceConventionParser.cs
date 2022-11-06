using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Impartial.Services.ScoresheetParser
{
    public class DanceConventionParser : IScoresheetParser
    {
        private Competition _newcomerCompetition;
        private Competition _noviceCompetition;
        private Competition _intermediateCompetition;
        private Competition _advancedCompetition;
        private Competition _allStarCompetition;

        public List<Judge> Judges { get; set; }
        public List<Score> Scores { get; set; }

        public DanceConventionParser(string filePath)
        {
            using (var doc = new PdfDocument(new PdfReader(filePath)))
            {

            }
        }

        public DanceConventionParser(List<string> filePaths)
        {
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
