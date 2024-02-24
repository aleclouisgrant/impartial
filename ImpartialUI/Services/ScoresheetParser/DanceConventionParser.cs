using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using Impartial;

namespace ImpartialUI.Services.ScoresheetParser
{
    public class DanceConventionParser : IScoresheetParser
    {
        private ICompetition _newcomerCompetition;
        private ICompetition _noviceCompetition;
        private ICompetition _intermediateCompetition;
        private ICompetition _advancedCompetition;
        private ICompetition _allStarCompetition;

        public DanceConventionParser(string prelimsPath, string filePath)
        {
            using (var doc = new PdfDocument(new PdfReader(filePath)))
            {

            }
        }

        public DanceConventionParser(string finalsPath)
        {
            if (finalsPath == null || finalsPath == String.Empty || !File.Exists(finalsPath))
                throw new FileNotFoundException();
        }

        public DanceConventionParser(List<string> filePaths)
        {
        }

        public ICompetition GetCompetition(Division division)
        {
            throw new NotImplementedException();
        }

        public List<Division> GetDivisions()
        {
            throw new NotImplementedException();
        }

        public List<IJudge> GetFinalsJudgesByDivision(Division division)
        {
            throw new NotImplementedException();
        }
    }
}
