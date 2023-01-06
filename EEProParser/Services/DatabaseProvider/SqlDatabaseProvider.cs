using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MongoDB.Driver.Core.Configuration;
using static iText.IO.Image.Jpeg2000ImageData;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Impartial
{
    public class SqlDatabaseProvider : IDatabaseProvider
    {
        string _connectionString;
        SqlHelper _helper;

        public SqlDatabaseProvider(string connectionString)
        {
            _connectionString = connectionString;
            _helper = new SqlHelper(connectionString);
        }

        public async Task InsertCompetitionAsync(Competition competition)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_Insert", competition);
        }
        public async Task UpdateCompetitionAsync(Guid id, Competition competition)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_Update", competition);
        }
        public async Task<Competition> GetCompetitionByIdAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<Competition, dynamic>(storedProcedure: "dbo.Competitions_GetById", new { Id = id });
            return results.FirstOrDefault();
        }
        public async Task<IEnumerable<Competition>> GetAllCompetitionsAsync()
        {
            return await _helper.LoadDataAsync<Competition, dynamic>(storedProcedure: "dbo.Competitions_GetAll", new { });
        }
        public async Task DeleteCompetitionAsync(Competition competition)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_DeleteById", new { Id = competition.Id });
        }
        public async Task DeleteAllCompetitionsAsync()
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_DeleteAll", new { });
        }

        public async Task InsertCompetitorAsync(Competitor competitor)
        {
            var c = new
            {
                Id = competitor.Id,
                WsdcId = competitor.WsdcId,
                FirstName = competitor.FirstName,
                LastName = competitor.LastName,
                LeaderRating = competitor.LeadStats.Rating,
                LeaderVariance = competitor.LeadStats.Variance,
                FollowerRating = competitor.FollowStats.Rating,
                FollowerVariance = competitor.FollowStats.Variance,
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_Insert", c);
        }
        public async Task UpdateCompetitorAsync(Guid id, Competitor competitor)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_Update", competitor);
        }
        public async Task<Competitor> GetCompetitorByIdAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<Competitor, dynamic>(storedProcedure: "dbo.Competitors_GetById", new { Id = id });
            return results.FirstOrDefault();
        }
        public async Task<Competitor> GetCompetitorByNameAsync(string firstName, string lastName)
        {
            var results = await _helper.LoadDataAsync<Competitor, dynamic>(storedProcedure: "dbo.Competitors_GetByName", new { FirstName = firstName, LastName = lastName });
            return results.FirstOrDefault();
        }
        public async Task<IEnumerable<Competitor>> GetAllCompetitorsAsync()
        {
            return await _helper.LoadDataAsync<Competitor, dynamic>(storedProcedure: "dbo.Competitors_GetAll", new { });
        }
        public async Task DeleteCompetitorAsync(Competitor competitor)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_DeleteById", new { Id = competitor.Id });
        }
        public async Task DeleteAllCompetitorsAsync()
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_DeleteAll", new { });
        }

        public async Task InsertJudgeAsync(Judge judge)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Judges_Insert", judge);
        }
        public async Task UpdateJudgeAsync(Guid id, Judge judge)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Judges_Update", judge);
        }
        public async Task<Judge> GetJudgeByIdAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<Judge, dynamic>(storedProcedure: "dbo.Judges_GetById", new { Id = id });
            return results.FirstOrDefault();
        }
        public async Task<Judge> GetJudgeByNameAsync(string firstName, string lastName)
        {
            var results = await _helper.LoadDataAsync<Judge, dynamic>(storedProcedure: "dbo.Judges_GetByName", new { FirstName = firstName, LastName = lastName });
            return results.FirstOrDefault();
        }
        public async Task<IEnumerable<Judge>> GetAllJudgesAsync()
        {
            return await _helper.LoadDataAsync<Judge, dynamic>(storedProcedure: "dbo.Judges_GetAll", new { });
        }
        public async Task DeleteJudgeAsync(Judge judge)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Judges_DeleteById", new { Id = judge.Id });
        }
        public async Task DeleteAllJudgesAsync()
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Judges_DeleteAll", new { });
        }
    }
}
