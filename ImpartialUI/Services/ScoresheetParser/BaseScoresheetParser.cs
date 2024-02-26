using Impartial;
using ImpartialUI.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace ImpartialUI.Services.ScoresheetParser
{
    public abstract class ScoresheetParserBase : IScoresheetParser
    {
        public string PrelimsSheetDoc { get; set; }
        public string QuartersSheetDoc { get; set; }
        public string SemisSheetDoc { get; set; }
        public string FinalsSheetDoc { get; set; }

        public ScoresheetParserBase(string prelimsPath = null, string finalsPath = null)
        {
            bool prelimPathFound = !(prelimsPath == null || prelimsPath == string.Empty || !File.Exists(prelimsPath));
            bool finalsPathFound = !(finalsPath == null || finalsPath == string.Empty || !File.Exists(finalsPath));

            if (!prelimPathFound && !finalsPathFound)
                throw new FileNotFoundException();

            PrelimsSheetDoc = prelimPathFound ? File.ReadAllText(prelimsPath).Replace("\n", "").Replace("\r", "") : null;
            FinalsSheetDoc = finalsPathFound ? File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "") : null;
        }

        public ScoresheetParserBase(string prelimsPath = null, string semisPath = null, string finalsPath = null)
        {
            bool prelimPathFound = !(prelimsPath == null || prelimsPath == string.Empty || !File.Exists(prelimsPath));
            bool semisPathFound = !(semisPath == null || semisPath == string.Empty || !File.Exists(semisPath));
            bool finalsPathFound = !(finalsPath == null || finalsPath == string.Empty || !File.Exists(finalsPath));

            if (!prelimPathFound && !semisPathFound && !finalsPathFound)
                throw new FileNotFoundException();

            PrelimsSheetDoc = prelimPathFound ? File.ReadAllText(prelimsPath).Replace("\n", "").Replace("\r", "") : null;
            SemisSheetDoc = semisPathFound ? File.ReadAllText(semisPath).Replace("\n", "").Replace("\r", "") : null;
            FinalsSheetDoc = finalsPathFound ? File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "") : null;
        }

        public ScoresheetParserBase(string prelimsPath = null, string quartersPath = null, string semisPath = null, string finalsPath = null)
        {
            bool prelimPathFound = !(prelimsPath == null || prelimsPath == string.Empty || !File.Exists(prelimsPath));
            bool quarterPathFound = !(quartersPath == null || quartersPath == string.Empty || !File.Exists(quartersPath));
            bool semisPathFound = !(semisPath == null || semisPath == string.Empty || !File.Exists(semisPath));
            bool finalsPathFound = !(finalsPath == null || finalsPath == string.Empty || !File.Exists(finalsPath));

            if (!prelimPathFound && !quarterPathFound && !semisPathFound && !finalsPathFound)
                throw new FileNotFoundException();

            PrelimsSheetDoc = prelimPathFound ? File.ReadAllText(prelimsPath).Replace("\n", "").Replace("\r", "") : null;
            QuartersSheetDoc = prelimPathFound ? File.ReadAllText(quartersPath).Replace("\n", "").Replace("\r", "") : null;
            SemisSheetDoc = semisPathFound ? File.ReadAllText(semisPath).Replace("\n", "").Replace("\r", "") : null;
            FinalsSheetDoc = finalsPathFound ? File.ReadAllText(finalsPath).Replace("\n", "").Replace("\r", "") : null;
        }

        public virtual IPairedPrelimCompetition GetPairedPrelimCompetition(Division division, Round round)
        {
            return PrelimsSheetDoc != null ?
                new PairedPrelimCompetition(round, GetPrelimCompetition(division, round, Role.Leader), GetPrelimCompetition(division, round, Role.Follower))
                : new PairedPrelimCompetition(round, null, null);
        }

        public virtual ICompetition GetCompetition(Division division)
        {
            var competition = new Competition(danceConventionId: Guid.Empty, name: GetName(), date: DateTime.MinValue, division: division)
            {
                FinalCompetition = GetFinalCompetition(division),
                PairedPrelimCompetitions = new List<IPairedPrelimCompetition>()
            };

            var prelims = GetPairedPrelimCompetition(division, Round.Prelims);
            var quarters = GetPairedPrelimCompetition(division, Round.Quarterfinals);
            var semis = GetPairedPrelimCompetition(division, Round.Semifinals);

            if (prelims.LeaderPrelimCompetition != null)
                competition.PairedPrelimCompetitions.Add(prelims);

            if (quarters.LeaderPrelimCompetition != null)
                competition.PairedPrelimCompetitions.Add(quarters);

            if (semis.LeaderPrelimCompetition != null)
                competition.PairedPrelimCompetitions.Add(semis);

            return competition;
        }

        public abstract IPrelimCompetition? GetPrelimCompetition(Division division, Round round, Role role);
        public abstract IFinalCompetition? GetFinalCompetition(Division division);
        public abstract List<Division> GetDivisions();
        public abstract string GetName();
    }
}
