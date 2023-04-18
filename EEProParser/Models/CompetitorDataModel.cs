using System;
using System.Collections.Generic;

namespace Impartial
{
    public class CompetitorDataModel
    {
        public Guid CompetitorId { get; set; }
        public Competitor Competitor { get; set; }
        public int WsdcPoints { get; set; }
        public List<CompetitionHistory> CompetitionHistory { get; set; } = new();

        public CompetitorDataModel(Competitor competitor, int points) 
        {
            CompetitorId = competitor.Id;
            Competitor = competitor;
            WsdcPoints = points;
        }

        public CompetitorDataModel(Guid competitorId, int wsdcPoints)
        {
            CompetitorId = competitorId;
            WsdcPoints = wsdcPoints;
        }
    }
}
