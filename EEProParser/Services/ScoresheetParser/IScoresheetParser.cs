using System.Collections.Generic;

namespace Impartial
{
    public interface IScoresheetParser
    {
        public List<Division> GetDivisions();
        public Competition GetCompetition(Division division);
    }
}
