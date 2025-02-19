namespace Impartial
{
    public interface ICompetitorRegistration
    {
        ICompetitor Competitor { get; set; }
        string BibNumber { get; set; }
    }
}
