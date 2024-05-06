using Impartial;
using Impartial.Models;

namespace ImpartialUI.Models
{
    public class CompetitorRegistration : ICompetitorRegistration
    {
        public ICompetitor Competitor { get; set; }
        public string BibNumber { get; set; }
    }
}
