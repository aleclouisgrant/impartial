using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Impartial;
using ImpartialUI.Models;
using ImpartialUI.Services.DatabaseProvider.Helpers;
using MongoDB.Driver.Core.Configuration;
using static System.Formats.Asn1.AsnWriter;
using static iText.IO.Image.Jpeg2000ImageData;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace ImpartialUI.Services.DatabaseProvider
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

        public async Task UpsertCompetitionAsync(ICompetition competition)
        {
            var c = new
            {
                competition.Id,
                competition.Name,
                competition.Date,
                Division = competition.Division.ToString(),
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_Upsert", c);

            await DeleteScoresByCompIdAsync(competition.Id);

            foreach (IFinalScore score in competition.Scores)
            {
                await UpsertScoreAsync(score);
            }

            foreach (PrelimScore score in competition.LeaderPrelimScores)
            {
                await UpsertPrelimScoreAsync(score);
            }

            foreach (PrelimScore score in competition.FollowerPrelimScores)
            {
                await UpsertPrelimScoreAsync(score);
            }
        }
        public async Task<ICompetition> GetCompetitionAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<ICompetition, dynamic>(storedProcedure: "dbo.Competitions_GetById", new { Id = id });
            var comp = results.FirstOrDefault();

            if (comp != null)
            {
                comp.Scores = (await GetScoresByCompAsync(comp.Id, comp.Division)).ToList();

                var prelimScores = (await GetPrelimScoresByCompAsync(comp.Id, comp.Division)).ToList();
                comp.LeaderPrelimScores = prelimScores.Where(s => s.Role == Role.Leader).ToList();
                comp.FollowerPrelimScores = prelimScores.Where(s => s.Role == Role.Follower).ToList();
            }

            return comp;
        }
        public async Task<IEnumerable<ICompetition>> GetAllCompetitionsAsync()
        {
            IEnumerable<ICompetition> comps = await _helper.LoadDataAsync<ICompetition, dynamic>(storedProcedure: "dbo.Competitions_GetAll", new { });

            foreach (var comp in comps)
            {
                comp.Scores = (await GetScoresByCompAsync(comp.Id, comp.Division)).ToList();
                var prelimScores = (await GetPrelimScoresByCompAsync(comp.Id, comp.Division)).ToList();
                comp.LeaderPrelimScores = prelimScores.Where(s => s.Role == Role.Leader).ToList();
                comp.FollowerPrelimScores = prelimScores.Where(s => s.Role == Role.Follower).ToList();
            }

            return comps;
        }
        public async Task DeleteCompetitionAsync(ICompetition competition)
        {
            await DeleteScoresByCompIdAsync(competition.Id);
            await DeletePrelimScoresByCompIdAsync(competition.Id);
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_DeleteById", new { competition.Id });
        }
        public async Task DeleteAllCompetitionsAsync()
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_DeleteAll", new { });
        }

        public async Task UpsertCompetitorAsync(ICompetitor competitor)
        {
            var c = new
            {
                competitor.Id,
                competitor.WsdcId,
                competitor.FirstName,
                competitor.LastName,
                LeaderRating = competitor.LeadStats.Rating,
                LeaderVariance = competitor.LeadStats.Variance,
                FollowerRating = competitor.FollowStats.Rating,
                FollowerVariance = competitor.FollowStats.Variance,
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_Upsert", c);
        }
        public async Task<ICompetitor> GetCompetitorAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<ICompetitor, dynamic>(storedProcedure: "dbo.Competitors_GetById", new { Id = id });
            return results.FirstOrDefault();
        }
        public async Task<ICompetitor> GetCompetitorAsync(string firstName, string lastName)
        {
            var results = await _helper.LoadDataAsync<ICompetitor, dynamic>(storedProcedure: "dbo.Competitors_GetByName", new { FirstName = firstName, LastName = lastName });
            return results.FirstOrDefault();
        }
        public async Task<IEnumerable<ICompetitor>> GetAllCompetitorsAsync()
        {
            return (await _helper.LoadDataAsync<ICompetitor, dynamic>(storedProcedure: "dbo.Competitors_GetAll", new { })).OrderBy(c => c.FullName);
        }
        public async Task DeleteCompetitorAsync(ICompetitor competitor)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_DeleteById", new { competitor.Id });
        }
        public async Task DeleteAllCompetitorsAsync()
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_DeleteAll", new { });
        }

        public async Task UpsertJudgeAsync(IJudge judge)
        {
            var j = new
            {
                judge.Id,
                judge.FirstName,
                judge.LastName,
                judge.Accuracy,
                judge.Top5Accuracy,
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.Judges_Upsert", j);

            if (judge.Scores?.Count > 0)
            {
                foreach (IFinalScore score in judge.Scores)
                {
                    await UpsertScoreAsync(score);
                }
            }
        }
        public async Task<IJudge> GetJudgeAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<IJudge, dynamic>(storedProcedure: "dbo.Judges_GetById", new { Id = id });
            return results.FirstOrDefault();
        }
        public async Task<IJudge> GetJudgeAsync(string firstName, string lastName)
        {
            var results = await _helper.LoadDataAsync<IJudge, dynamic>(storedProcedure: "dbo.Judges_GetByName", new { FirstName = firstName, LastName = lastName });
            return results.FirstOrDefault();
        }
        public async Task<IEnumerable<IJudge>> GetAllJudgesAsync()
        {
            return await _helper.LoadDataAsync<IJudge, dynamic>(storedProcedure: "dbo.Judges_GetAll", new { });
        }
        public async Task DeleteJudgeAsync(IJudge judge)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Judges_DeleteById", new { judge.Id });
        }
        public async Task DeleteAllJudgesAsync()
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Judges_DeleteAll", new { });
        }

        public async Task UpsertScoreAsync(IFinalScore score)
        {
            var s = new
            {
                score.Id,
                CompetitionId = score.Competition.Id,
                JudgeId = score.Judge.Id,
                LeaderId = score.Leader.Id,
                FollowerId = score.Follower.Id,
                JudgePlacement = score.Score,
                score.Placement,
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.Scores_Upsert", s);
        }
        public async Task<IFinalScore> GetScoreByIdAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<IFinalScore, dynamic>(storedProcedure: "dbo.Scores_GetById", new { Id = id });
            return results.FirstOrDefault();
        }
        public async Task<IEnumerable<IFinalScore>> GetScoresByCompAsync(Guid competitionId, Division division)
        {
            //not using Division right now but will need to update stored procedure to do so
            return await _helper.LoadDataAsync<IFinalScore, dynamic>(storedProcedure: "dbo.Scores_GetByCompId", new { Id = competitionId, Division = division });
        }
        public async Task DeleteScoresByCompIdAsync(Guid competitionId)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Scores_DeleteAllByCompId", new { CompetitionId = competitionId });
        }

        public async Task UpsertPrelimScoreAsync(PrelimScore score)
        {
            var s = new
            {
                score.Id,
                CompetitionId = score.Competition.Id,
                JudgeId = score.Judge.Id,
                CompetitorId = score.Competitor.Id,
                Role = score.Role.ToString(),
                score.Finaled,
                score.CallbackScore,
                score.RawScore,
                score.Round
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.PrelimScores_Upsert", s);
        }
        public async Task<PrelimScore> GetPrelimScoreByIdAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<PrelimScore, dynamic>(storedProcedure: "dbo.PrelimScores_GetById", new { Id = id });
            return results.FirstOrDefault();
        }
        public async Task<IEnumerable<PrelimScore>> GetPrelimScoresByCompAsync(Guid competitionId, Division division)
        {
            //not using Division right now but will need to update stored procedure to do so
            return await _helper.LoadDataAsync<PrelimScore, dynamic>(storedProcedure: "dbo.PrelimScores_GetByCompId", new { Id = competitionId, Division = division });
        }
        public async Task DeletePrelimScoresByCompIdAsync(Guid competitionId)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.PrelimScores_DeleteAllByCompId", new { CompetitionId = competitionId });
        }
    }
}
