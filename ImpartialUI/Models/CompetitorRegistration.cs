using Impartial;

namespace ImpartialUI
{
    public class CompetitorRegistration : ICompetitorRegistration
    {
        public ICompetitor Competitor { get; set; }
        public string BibNumber { get; set; }

        public CompetitorRegistration(ICompetitor competitor, string bibNumber) 
        {
            Competitor = competitor;
            BibNumber = bibNumber;
        }
    }
}
