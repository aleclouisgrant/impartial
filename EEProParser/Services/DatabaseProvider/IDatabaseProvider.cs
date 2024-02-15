using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Impartial
{
    public interface IDatabaseProvider
    {
        Task UpsertCompetitorAsync(Competitor competitor);
        Task<Competitor?> GetCompetitorAsync(Guid id);
        Task<Competitor?> GetCompetitorAsync(string firstName, string lastName);
        Task<IEnumerable<Competitor>> GetAllCompetitorsAsync();
        Task DeleteCompetitorAsync(Guid id);
        Task DeleteAllCompetitorsAsync();

        Task UpsertJudgeAsync(Judge judge);
        Task<Judge?> GetJudgeAsync(Guid id);
        Task<Judge?> GetJudgeAsync(string firstName, string lastName);
        Task<IEnumerable<Judge>> GetAllJudgesAsync();
        Task DeleteJudgeAsync(Judge judge);
        Task DeleteAllJudgesAsync();

        Task UpsertDanceConventionAsync(DanceConvention convention);
        Task<DanceConvention?> GetDanceConventionAsync(Guid id);
        Task<IEnumerable<DanceConvention>> GetAllDanceConventionsAsync();
        Task DeleteDanceConventionAsync(Guid id);
        Task DeleteAllDanceConventionsAsync();

        Task UpsertCompetitionAsync(Competition competition, Guid danceConventionId);
        Task<Competition?> GetCompetitionAsync(Guid id);
        Task<IEnumerable<Competition>> GetAllCompetitionsAsync();
        Task DeleteCompetitionAsync(Guid id);
        Task DeleteAllCompetitionsAsync();
    }
}
