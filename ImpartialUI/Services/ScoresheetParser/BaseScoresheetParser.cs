using HtmlAgilityPack;
using Impartial.Enums;
using Impartial;
using ImpartialUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImpartialUI.Services.ScoresheetParser
{
    public abstract class ScoresheetParserBase : IScoresheetParser
    {
        public string PrelimsSheetDoc { get; set; }
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

        public IPairedPrelimCompetition GetPairedPrelimCompetition(Division division, Round round)
        {
            return PrelimsSheetDoc != null ?
                new PairedPrelimCompetition(round, GetPrelimCompetition(division, round, Role.Leader), GetPrelimCompetition(division, round, Role.Follower))
                : new PairedPrelimCompetition(round, null, null);
        }

        public ICompetition GetCompetition(Division division)
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
