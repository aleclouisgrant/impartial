using System.Collections.Generic;

namespace Impartial
{
    public interface IScoresheetParser
    {
        public List<Division> GetDivisions();
        public ICompetition GetCompetition(Division division);
    }
}
