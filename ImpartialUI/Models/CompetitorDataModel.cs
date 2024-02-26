using Impartial;
using System;
using System.Collections.Generic;

namespace ImpartialUI.Models
{
    public class CompetitorDataModel
    {
        public Guid CompetitorId { get; set; }
        public ICompetitor Competitor { get; set; }
        public List<CompetitionHistory> CompetitionHistory { get; set; } = new();

        public CompetitorDataModel(ICompetitor competitor)
        {
            CompetitorId = competitor.CompetitorId;
            Competitor = competitor;
        }

        public CompetitorDataModel(Guid competitorId)
        {
            CompetitorId = competitorId;
        }
    }
}
