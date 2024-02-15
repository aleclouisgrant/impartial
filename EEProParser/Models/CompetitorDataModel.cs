using System;
using System.Collections.Generic;

namespace Impartial
{
    public class CompetitorDataModel
    {
        public Guid CompetitorId { get; set; }
        public Competitor Competitor { get; set; }
        public List<CompetitionHistory> CompetitionHistory { get; set; } = new();

        public CompetitorDataModel(Competitor competitor) 
        {
            CompetitorId = competitor.Id;
            Competitor = competitor;
        }

        public CompetitorDataModel(Guid competitorId)
        {
            CompetitorId = competitorId;
        }
    }
}
