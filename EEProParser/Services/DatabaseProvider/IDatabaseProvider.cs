using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Impartial
{
    public interface IDatabaseProvider
    {
        Task UpsertCompetitorAsync(ICompetitor competitor);
        Task<ICompetitor?> GetCompetitorAsync(Guid id);
        Task<ICompetitor?> GetCompetitorAsync(string firstName, string lastName);
        Task<IEnumerable<ICompetitor>> GetAllCompetitorsAsync();
        Task DeleteCompetitorAsync(Guid id);
        Task DeleteAllCompetitorsAsync();

        Task UpsertJudgeAsync(IJudge judge);
        Task<IJudge?> GetJudgeAsync(Guid id);
        Task<IJudge?> GetJudgeAsync(string firstName, string lastName);
        Task<IEnumerable<IJudge>> GetAllJudgesAsync();
        Task DeleteJudgeAsync(Guid id);
        Task DeleteAllJudgesAsync();

        Task UpsertDanceConventionAsync(IDanceConvention convention);
        Task<IDanceConvention?> GetDanceConventionAsync(Guid id);
        Task<IEnumerable<IDanceConvention>> GetAllDanceConventionsAsync();
        Task DeleteDanceConventionAsync(Guid id);
        Task DeleteAllDanceConventionsAsync();

        Task UpsertCompetitionAsync(ICompetition competition, Guid danceConventionId);
        Task<ICompetition?> GetCompetitionAsync(Guid id);
        Task<IEnumerable<ICompetition>> GetAllCompetitionsAsync();
        Task DeleteCompetitionAsync(Guid id);
        Task DeleteAllCompetitionsAsync();
    }
}
