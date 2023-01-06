using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Impartial
{
    public interface IDatabaseProvider
    {
        Task InsertCompetitionAsync(Competition competition);
        Task UpdateCompetitionAsync(Guid id, Competition competition);
        Task<Competition> GetCompetitionByIdAsync(Guid id);
        Task DeleteCompetitionAsync(Competition competition);
        Task<IEnumerable<Competition>> GetAllCompetitionsAsync();
        Task DeleteAllCompetitionsAsync();

        Task InsertCompetitorAsync(Competitor competitor);
        Task UpdateCompetitorAsync(Guid id, Competitor competitor);
        Task<Competitor> GetCompetitorByIdAsync(Guid id);
        Task<Competitor> GetCompetitorByNameAsync(string firstName, string lastName);
        Task DeleteCompetitorAsync(Competitor competitor);
        Task<IEnumerable<Competitor>> GetAllCompetitorsAsync();
        Task DeleteAllCompetitorsAsync();

        Task InsertJudgeAsync(Judge judge);
        Task UpdateJudgeAsync(Guid id, Judge judge);
        Task<Judge> GetJudgeByIdAsync(Guid id);
        Task<Judge> GetJudgeByNameAsync(string firstName, string lastName);
        Task DeleteJudgeAsync(Judge judge);
        Task<IEnumerable<Judge>> GetAllJudgesAsync();
        Task DeleteAllJudgesAsync();
    }
}
