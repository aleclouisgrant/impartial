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

        public DanceConventionParser(string prelimsPath, string finalsPath)
        {
            using (var doc = new PdfDocument(new PdfReader(finalsPath)))
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

        public IPrelimCompetition GetPrelimCompetition(Division division, Round round, Role role)
        {
            throw new NotImplementedException();
        }

        public IPairedPrelimCompetition GetPairedPrelimCompetition(Division division, Round round)
        {
            throw new NotImplementedException();
        }

        public IFinalCompetition GetFinalCompetition(Division division)
        {
            throw new NotImplementedException();
        }

        public ICompetition GetCompetition(Division division)
        {
            throw new NotImplementedException();
        }
    }
}
