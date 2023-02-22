using System;
using System.Collections.Generic;

namespace Impartial
{
    public class CompetitorDataModel
    {
        public Guid CompetitorId { get; set; }
        public Competitor Competitor { get; set; }
        public int WsdcPoints { get; set; }
        private List<int> CompetitionHistory { get; set; } = new();

        public CompetitorDataModel(Competitor competitor, int points) 
        {
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
