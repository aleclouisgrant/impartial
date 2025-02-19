namespace Impartial
{
    public interface IScoresheetParser
    {
        public IPrelimCompetition GetPrelimCompetition(Division division, Round round, Role role);
        public IPairedPrelimCompetition GetPairedPrelimCompetition(Division division, Round round);
        public IFinalCompetition GetFinalCompetition(Division division);
        public ICompetition GetCompetition(Division division);
    }
}
