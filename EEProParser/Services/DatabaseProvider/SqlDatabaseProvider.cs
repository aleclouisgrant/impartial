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

        public async Task UpsertCompetitionAsync(Competition competition)
        {
            var c = new
            {
                Id = competition.Id,
                Name = competition.Name,
                Date = competition.Date,
                Division = competition.Division.ToString(),
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_Upsert", c);

            await DeleteScoresByCompIdAsync(competition.Id);

            foreach (Score score in competition.Scores)
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
        public async Task<Competition> GetCompetitionByIdAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<Competition, dynamic>(storedProcedure: "dbo.Competitions_GetById", new { Id = id });
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
        public async Task<IEnumerable<Competition>> GetAllCompetitionsAsync()
        {
            IEnumerable<Competition> comps = await _helper.LoadDataAsync<Competition, dynamic>(storedProcedure: "dbo.Competitions_GetAll", new { });

            foreach (var comp in comps)
            {
                comp.Scores = (await GetScoresByCompAsync(comp.Id, comp.Division)).ToList();
                var prelimScores = (await GetPrelimScoresByCompAsync(comp.Id, comp.Division)).ToList();
                comp.LeaderPrelimScores = prelimScores.Where(s => s.Role == Role.Leader).ToList();
                comp.FollowerPrelimScores = prelimScores.Where(s => s.Role == Role.Follower).ToList();
            }

            return comps;
        }
        public async Task DeleteCompetitionAsync(Competition competition)
        {
            await DeleteScoresByCompIdAsync(competition.Id);
            await DeletePrelimScoresByCompIdAsync(competition.Id);
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_DeleteById", new { Id = competition.Id });
        }
        public async Task DeleteAllCompetitionsAsync()
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitions_DeleteAll", new { });
        }

        public async Task UpsertCompetitorAsync(Competitor competitor)
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

            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_Upsert", c);
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
            return (await _helper.LoadDataAsync<Competitor, dynamic>(storedProcedure: "dbo.Competitors_GetAll", new { })).OrderBy(c => c.FullName);
        }
        public async Task DeleteCompetitorAsync(Competitor competitor)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_DeleteById", new { Id = competitor.Id });
        }
        public async Task DeleteAllCompetitorsAsync()
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Competitors_DeleteAll", new { });
        }

        public async Task UpsertJudgeAsync(Judge judge)
        {
            var j = new
            {
                Id = judge.Id,
                FirstName = judge.FirstName,
                LastName = judge.LastName,
                Accuracy = judge.Accuracy,
                Top5Accuracy = judge.Top5Accuracy,
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.Judges_Upsert", j);

            if (judge.Scores?.Count > 0)
            {
                foreach (Score score in judge.Scores)
                {
                    await UpsertScoreAsync(score);
                }
            }
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

        public async Task UpsertScoreAsync(Score score)
        {
            var s = new
            {
                Id = score.Id,
                CompetitionId = score.Competition.Id,
                JudgeId = score.Judge.Id,
                LeaderId = score.Leader.Id,
                FollowerId = score.Follower.Id,
                JudgePlacement = score.Placement,
                ActualPlacement = score.ActualPlacement,
            };

            await _helper.SaveDataAsync(storedProcedure: "dbo.Scores_Upsert", s);
        }
        public async Task<Score> GetScoreByIdAsync(Guid id)
        {
            var results = await _helper.LoadDataAsync<Score, dynamic>(storedProcedure: "dbo.Scores_GetById", new { Id = id });
            return results.FirstOrDefault();
        }
        public async Task<IEnumerable<Score>> GetScoresByCompAsync(Guid competitionId, Division division)
        {
            //not using Division right now but will need to update stored procedure to do so
            return await _helper.LoadDataAsync<Score, dynamic>(storedProcedure: "dbo.Scores_GetByCompId", new { Id = competitionId, Division = division });
        }
        public async Task DeleteScoresByCompIdAsync(Guid competitionId)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.Scores_DeleteAllByCompId", new { CompetitionId = competitionId });
        }

        public async Task UpsertPrelimScoreAsync(PrelimScore score)
        {
            var s = new
            {
                Id = score.Id,
                CompetitionId = score.Competition.Id,
                JudgeId = score.Judge.Id,
                CompetitorId = score.Competitor.Id,
                Role = score.Role.ToString(),
                Finaled = score.Finaled,
                CallbackScore = score.CallbackScore,
                RawScore = score.RawScore,
                Round = score.Round
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
            return await _helper.LoadDataAsync<PrelimScore, dynamic>(storedProcedure: "dbo.PrelimScores_GetByCompId", new { Id = competitionId, Division = division});
        }
        public async Task DeletePrelimScoresByCompIdAsync(Guid competitionId)
        {
            await _helper.SaveDataAsync(storedProcedure: "dbo.PrelimScores_DeleteAllByCompId", new { CompetitionId = competitionId });
        }
    }
}
