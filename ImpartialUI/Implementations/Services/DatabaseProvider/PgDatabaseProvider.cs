using Impartial.Models.PgModels;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ImpartialUI;
using Impartial;

namespace ImpartialUI.Implementations.Services.DatabaseProvider
{
    public class PgDatabaseProvider : IDatabaseProvider, IDisposable
    {
        const string PG_USERS_TABLE_NAME = "users";
        const string PG_COMPETITOR_PROFILES_TABLE_NAME = "competitor_profiles";
        const string PG_COMPETITOR_REGISTRATIONS_TABLE_NAME = "competitor_registrations";
        const string PG_COMPETITOR_RECORDS_TABLE_NAME = "competitor_records";
        const string PG_JUDGE_PROFILES_TABLE_NAME = "judge_profiles";
        const string PG_DANCE_CONVENTIONS_TABLE_NAME = "dance_conventions";
        const string PG_COMPETITIONS_TABLE_NAME = "competitions";
        const string PG_PRELIM_COMPETITIONS_TABLE_NAME = "prelim_competitions";
        const string PG_FINAL_COMPETITIONS_TABLE_NAME = "final_competitions";
        const string PG_PRELIM_SCORES_TABLE_NAME = "prelim_scores";
        const string PG_FINAL_SCORES_TABLE_NAME = "final_scores";
        const string PG_PROMOTED_COMPETITORS_TABLE_NAME = "promoted_competitors";
        const string PG_PLACEMENTS_TABLE_NAME = "placements";

        string _connectionString;
        private NpgsqlDataSource _dataSource;

        public PgDatabaseProvider(string host, string user, string dbName, string port, string password)
        {
            _connectionString =
                string.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    host,
                    user,
                    dbName,
                    port,
                    password);

            _dataSource = NpgsqlDataSource.Create(_connectionString);
        }

        #region Helper
        private string CreateInsertQuery<U>(string table, U parameters)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = "INSERT INTO " + table;

            string columnNames = string.Empty;
            if (properties.Length != 0)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].GetValue(parameters) != null)
                    {
                        columnNames += properties[i].Name;
                        if (i != properties.Length)
                        {
                            columnNames += ", ";
                        }
                    }
                }

                if (columnNames != string.Empty)
                {
                    command += "(" + columnNames + ")";
                    command += " VALUES ";

                    int count = 1;
                    for (int i = 1; i <= properties.Length; i++)
                    {
                        command += "($" + count + ")";
                        if (i != properties.Length)
                        {
                            command += ", ";
                        }
                        count++;
                    }
                }
            }

            return command;
        }
        private string CreateUpsertQuery<U>(string table, U parameters, string conflictParameter)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = "INSERT INTO " + table;

            string columnNames = string.Empty;
            if (properties.Length != 0)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].GetValue(parameters) != null)
                    {
                        columnNames += properties[i].Name;
                        if (i != properties.Length)
                        {
                            columnNames += ", ";
                        }
                    }
                }

                if (columnNames != string.Empty)
                {
                    command += "(" + columnNames + ")";
                    command += " VALUES ";

                    int count = 1;
                    for (int i = 1; i <= properties.Length; i++)
                    {
                        command += "($" + count + ")";
                        if (i != properties.Length)
                        {
                            command += ", ";
                        }
                        count++;
                    }
                }
            }

            command += " ON CONFLICT (" + conflictParameter + ")";
            command += " DO UPDATE SET ";

            string excluded = string.Empty;
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].GetValue(parameters) != null)
                {
                    excluded += properties[i].Name + " = EXCLUDED." + properties[i].Name;
                    if (i != properties.Length)
                    {
                        columnNames += ", ";
                    }
                }
            }

            command += excluded + ";";

            return command;
        }

        private string CreateSelectQuery<U>(string table, U parameters)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();

            string columnNames = string.Empty;
            if (properties.Length != 0)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].GetValue(parameters) != null)
                    {
                        columnNames += properties[i].Name;
                        if (i != properties.Length)
                        {
                            columnNames += ", ";
                        }
                    }
                }
            }

            return "SELECT " + columnNames + " FROM " + table;
        }

        private async Task SaveDataAsync<U>(string table, U parameters)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = CreateInsertQuery(table, parameters);

            await using (var cmd = _dataSource.CreateCommand(command))
            {
                foreach (PropertyInfo property in properties)
                {
                    cmd.Parameters.AddWithValue(property.Name, property.GetValue(parameters));
                }

                await cmd.ExecuteNonQueryAsync();
            }
        }
        private async Task UpsertDataAsync<U>(string table, U parameters, string conflictParameter)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = CreateUpsertQuery(table, parameters, conflictParameter);

            await using (var cmd = _dataSource.CreateCommand(command))
            {
                foreach (PropertyInfo property in properties)
                {
                    cmd.Parameters.AddWithValue(property.Name, property.GetValue(parameters));
                }

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task<IEnumerable<T>> LoadDataAsync<T, U>(string table, U parameters)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = CreateSelectQuery(table, parameters);
            var data = new List<T>();

            await using (var cmd = _dataSource.CreateCommand(command))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                foreach (PropertyInfo property in properties)
                {
                    cmd.Parameters.AddWithValue(property.Name, property.GetValue(parameters));
                }

                while (await reader.ReadAsync())
                {
                    data.Add((T)reader.GetValue(0));
                }
            }

            return data;
        }
        private async Task<IEnumerable<T>> LoadAllDataAsync<T, U>(string table)
        {
            string command = "SELECT * FROM " + table;
            var data = new List<T>();

            await using (var cmd = _dataSource.CreateCommand(command))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    data.Add((T)reader.GetValue(0));
                }
            }

            return data;
        }
        #endregion

        public async Task UpsertCompetitorAsync(ICompetitor competitor)
        {
            var userModel = new PgUserModel
            {
                id = competitor.Id,
                first_name = competitor.FirstName,
                last_name = competitor.LastName,
            };

            var competitorProfileModel = new PgCompetitorProfileModel
            {
                user_id = competitor.Id,
                wsdc_id = competitor.WsdcId
            };

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await SaveDataAsync(PG_USERS_TABLE_NAME, userModel);
            await SaveDataAsync(PG_COMPETITOR_PROFILES_TABLE_NAME, competitorProfileModel);

            await transaction.CommitAsync();
        }
        public async Task<ICompetitor> GetCompetitorAsync(Guid id)
        {
            string query = "SELECT competitor_profiles.id, users.first_name, users.last_name, competitor_profiles.wsdc_id competitor_profiles.leader_rating competitor_profiles.follower_rating"
                + " FROM " + PG_USERS_TABLE_NAME + " LEFT JOIN " + PG_COMPETITOR_PROFILES_TABLE_NAME + " ON users.id = competitor_profiles.user_id"
                + " WHERE competitor_profiles.id = " + id;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid competitorId = reader.GetGuid(0);
                    string competitorFirstName = reader.GetString(1);
                    string competitorLastName = reader.GetString(2);
                    int competitorWsdcId = reader.GetInt32(3);
                    int leaderRating = reader.GetInt32(4);
                    int followerRating = reader.GetInt32(5);

                    return new ICompetitor(competitorId, competitorWsdcId, competitorFirstName, competitorLastName, leaderRating, 0, followerRating, 0);
                }
            }

            return null;
        }
        public async Task<ICompetitor> GetCompetitorAsync(string firstName, string lastName)
        {
            string query = "SELECT competitor_profiles.id, users.first_name, users.last_name, competitor_profiles.wsdc_id competitor_profiles.leader_rating competitor_profiles.follower_rating"
               + " FROM " + PG_USERS_TABLE_NAME + " LEFT JOIN " + PG_COMPETITOR_PROFILES_TABLE_NAME + " ON users.id = competitor_profiles.user_id"
               + " WHERE users.first_name = " + firstName + " AND users.last_name = " + lastName;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid competitorId = reader.GetGuid(0);
                    string competitorFirstName = reader.GetString(1);
                    string competitorLastName = reader.GetString(2);
                    int competitorWsdcId = reader.GetInt32(3);
                    int leaderRating = reader.GetInt32(4);
                    int followerRating = reader.GetInt32(5);

                    return new ICompetitor(competitorId, competitorWsdcId, competitorFirstName, competitorLastName, leaderRating, 0, followerRating, 0);
                }
            }

            return null;
        }
        public async Task<IEnumerable<ICompetitor>> GetAllCompetitorsAsync()
        {
            string query = "SELECT competitor_profiles.id, users.first_name, users.last_name, competitor_profiles.wsdc_id competitor_profiles.leader_rating competitor_profiles.follower_rating"
                + " FROM " + PG_USERS_TABLE_NAME + " LEFT JOIN " + PG_COMPETITOR_PROFILES_TABLE_NAME + " ON users.id = competitor_profiles.user_id";

            var results = new List<ICompetitor>();

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid competitorId = reader.GetGuid(0);
                    string competitorFirstName = reader.GetString(1);
                    string competitorLastName = reader.GetString(2);
                    int competitorWsdcId = reader.GetInt32(3);
                    int leaderRating = reader.GetInt32(4);
                    int followerRating = reader.GetInt32(5);

                    results.Add(new ICompetitor(competitorId, competitorWsdcId, competitorFirstName, competitorLastName, leaderRating, 0, followerRating, 0));
                }
            }

            return results.OrderBy(c => c.FullName);
        }
        public async Task DeleteCompetitorAsync(Guid id)
        {

        }
        public async Task DeleteAllCompetitorsAsync()
        {

        }

        public async Task UpsertJudgeAsync(IJudge judge)
        {
            var userModel = new PgUserModel
            {
                id = judge.Id,
                first_name = judge.FirstName,
                last_name = judge.LastName,
            };

            var judgeModel = new PgJudgeProfileModel
            {
                user_id = judge.Id,
            };

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await SaveDataAsync(PG_USERS_TABLE_NAME, userModel);
            await SaveDataAsync(PG_JUDGE_PROFILES_TABLE_NAME, judgeModel);

            await transaction.CommitAsync();
        }
        public async Task<IJudge> GetJudgeAsync(Guid id)
        {
            string query = "SELECT judge_profiles.id, users.first_name, users.last_name"
                + " FROM " + PG_USERS_TABLE_NAME + " LEFT JOIN " + PG_JUDGE_PROFILES_TABLE_NAME + " ON users.id = judge_profiles.user_id"
                + " WHERE judge_profiles.id = " + id;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid judgeId = reader.GetGuid(0);
                    string judgeFirstName = reader.GetString(1);
                    string judgeLastName = reader.GetString(2);

                    return new IJudge(judgeFirstName, judgeLastName, judgeId);
                }
            }

            return null;
        }
        public async Task<IJudge> GetJudgeAsync(string firstName, string lastName)
        {
            string query = "SELECT users.id, users.first_name, users.last_name"
                + " FROM " + PG_USERS_TABLE_NAME + " LEFT JOIN " + PG_JUDGE_PROFILES_TABLE_NAME + " ON users.id = judge_profiles.user_id"
               + " WHERE users.first_name = " + firstName + " AND users.last_name = " + lastName;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid judgeId = reader.GetGuid(0);
                    string judgeFirstName = reader.GetString(1);
                    string judgeLastName = reader.GetString(2);

                    return new IJudge(judgeFirstName, judgeLastName, judgeId);
                }
            }

            return null;
        }
        public async Task<IEnumerable<IJudge>> GetAllJudgesAsync()
        {
            string query = "SELECT judge_profiles.id, users.first_name, users.last_name"
                + " FROM " + PG_USERS_TABLE_NAME + " LEFT JOIN " + PG_JUDGE_PROFILES_TABLE_NAME + " ON users.id = judge_profiles.user_id";

            var results = new List<IJudge>();

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid judgeId = reader.GetGuid(0);
                    string judgeFirstName = reader.GetString(1);
                    string judgeLastName = reader.GetString(2);

                    results.Add(new IJudge(judgeFirstName, judgeLastName, judgeId));
                }
            }

            return results;
        }
        public async Task DeleteJudgeAsync(IJudge judge)
        {
        }
        public async Task DeleteAllJudgesAsync()
        {
        }

        public async Task UpsertDanceConventionAsync(IDanceConvention convention)
        {
            var pgDanceConventionModel = new PgDanceConventionModel
            {
                id = convention.Id,
                name = convention.Name,
                date = convention.Date,
            };

            await SaveDataAsync(PG_DANCE_CONVENTIONS_TABLE_NAME, pgDanceConventionModel);
        }
        public async Task<IDanceConvention> GetDanceConventionAsync(Guid id)
        {
            string query = "SELECT id, name, date FROM dance_conventions"
                + " WHERE id = " + id;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid danceConventionId = reader.GetGuid(0);
                    string danceConventionName = reader.GetString(1);
                    string danceConventionDateString = reader.GetString(2);

                    return new IDanceConvention(danceConventionName, DateTime.Parse(danceConventionDateString), danceConventionId);
                }
            }

            return null;
        }
        public async Task<IEnumerable<IDanceConvention>> GetAllDanceConventionsAsync()
        {
            string query = "SELECT id, name, date FROM dance_conventions";

            var results = new List<IDanceConvention>();

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid danceConventionId = reader.GetGuid(0);
                    string danceConventionName = reader.GetString(1);
                    string danceConventionDateString = reader.GetString(2);

                    results.Add(new IDanceConvention(danceConventionName, DateTime.Parse(danceConventionDateString), danceConventionId));
                }
            }

            foreach (var danceConvention in results)
            {
                string competitionQuery = "SELECT id, division FROM " + PG_COMPETITIONS_TABLE_NAME;

                await using (var cmd = _dataSource.CreateCommand(query))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid competitionId = reader.GetGuid(0);
                        string divisionString = reader.GetString(1);
                        var division = Util.GetDivisionFromString(divisionString);

                        danceConvention.Competitions.Add(new Competition(danceConvention, division, competitionId));
                    }
                }

                foreach (var competition in danceConvention.Competitions)
                {
                    string prelimCompetitionQuery = "SELECT id, division FROM " + PG_COMPETITIONS_TABLE_NAME;

                    await using (var cmd = _dataSource.CreateCommand(query))
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Guid competitionId = reader.GetGuid(0);
                            string divisionString = reader.GetString(1);
                            var division = Util.GetDivisionFromString(divisionString);

                            danceConvention.Competitions.Add(new Competition(danceConvention, division, competitionId));
                        }
                    }

                    string finalCompetitionQuery = "SELECT id, division FROM " + PG_COMPETITIONS_TABLE_NAME;

                    await using (var cmd = _dataSource.CreateCommand(query))
                    await using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Guid competitionId = reader.GetGuid(0);
                            string divisionString = reader.GetString(1);
                            var division = Util.GetDivisionFromString(divisionString);

                            danceConvention.Competitions.Add(new Competition(danceConvention, division, competitionId));
                        }
                    }
                }
            }

            return results;
        }
        public async Task DeleteDanceConventionAsync(Guid id)
        {

        }
        public async Task DeleteAllDanceConventionsAsync()
        {

        }

        public async Task UpsertCompetitionAsync(ICompetition competition, Guid danceConventionId)
        {
            var pgCompetitionModel = new PgCompetitionModel
            {
                dance_convention_id = danceConventionId,
                division = competition.Division,
                leader_tier = competition.LeaderTier,
                follower_tier = competition.FollowerTier
            };

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await SaveDataAsync(PG_COMPETITIONS_TABLE_NAME, pgCompetitionModel);

            // TODO: need to add competitor registrations

            // TODO: need to add competitor records

            foreach (var pairedPrelimCompetition in competition.PairedPrelimCompetitions)
            {
                var leaderPrelimCompetitionModel = new PgPrelimCompetitionModel
                {
                    competition_id = competition.Id,
                    date_time = pairedPrelimCompetition.LeaderPrelimCompetition.DateTime,
                    role = Role.Leader,
                    round = pairedPrelimCompetition.LeaderPrelimCompetition.Round,
                };

                foreach (var promotedCompetitor in pairedPrelimCompetition.LeaderPrelimCompetition.PromotedCompetitors)
                {
                    var leaderPrelimPromotedCompetitorsModel = new PgPromotedCompetitorModel
                    {
                        prelim_competition_id = leaderPrelimCompetitionModel.id,
                        competitor_id = promotedCompetitor.Id
                    };
                }

                var followerPrelimCompetitionModel = new PgPrelimCompetitionModel
                {
                    competition_id = competition.Id,
                    date_time = pairedPrelimCompetition.FollowerPrelimCompetition.DateTime,
                    role = Role.Follower,
                    round = pairedPrelimCompetition.FollowerPrelimCompetition.Round,
                };

                foreach (var promotedCompetitor in pairedPrelimCompetition.FollowerPrelimCompetition.PromotedCompetitors)
                {
                    var followerPrelimPromotedCompetitorsModel = new PgPromotedCompetitorModel
                    {
                        prelim_competition_id = followerPrelimCompetitionModel.id,
                        competitor_id = promotedCompetitor.Id
                    };
                }

                await SaveDataAsync(PG_PRELIM_COMPETITIONS_TABLE_NAME, leaderPrelimCompetitionModel);
                await SaveDataAsync(PG_PRELIM_COMPETITIONS_TABLE_NAME, followerPrelimCompetitionModel);

                foreach (var prelimScore in pairedPrelimCompetition.LeaderPrelimCompetition.PrelimScores)
                {
                    var scoreModel = new PgPrelimScoreModel
                    {
                        id = prelimScore.Id,
                        prelim_competition_id = pairedPrelimCompetition.LeaderPrelimCompetition.Id,
                        competitor_id = prelimScore.Competitor.Id,
                        judge_id = prelimScore.Judge.Id,
                        callbackscore = prelimScore.CallbackScore,
                    };

                    await SaveDataAsync(PG_PRELIM_SCORES_TABLE_NAME, prelimScore);
                }

                foreach (var prelimScore in pairedPrelimCompetition.FollowerPrelimCompetition.PrelimScores)
                {
                    var scoreModel = new PgPrelimScoreModel
                    {
                        id = prelimScore.Id,
                        prelim_competition_id = pairedPrelimCompetition.FollowerPrelimCompetition.Id,
                        competitor_id = prelimScore.Competitor.Id,
                        judge_id = prelimScore.Judge.Id,
                        callbackscore = prelimScore.CallbackScore,
                    };

                    await SaveDataAsync(PG_PRELIM_SCORES_TABLE_NAME, prelimScore);
                }
            }

            if (competition.FinalCompetition != null)
            {
                var finalCompetitionModel = new PgFinalCompetitionModel
                {
                    competition_id = competition.Id,
                    datetime = competition.FinalCompetition.DateTime
                };

                foreach (var couple in competition.FinalCompetition.Couples)
                {
                    var pgPlacementModel = new PgPlacementModel
                    {
                        final_competition_id = competition.FinalCompetition.Id,
                        leader_id = couple.Leader.Id,
                        follower_id = couple.Follower.Id,
                        placement = couple.ActualPlacement
                    };

                    await SaveDataAsync(PG_PLACEMENTS_TABLE_NAME, pgPlacementModel);
                }

                foreach (var finalScore in competition.FinalCompetition.FinalScores)
                {
                    var pgFinalScoreModel = new PgFinalScoreModel
                    {
                        final_competition_id = competition.FinalCompetition.Id,
                        judge_id = finalScore.Judge.Id,
                        leader_id = finalScore.Leader.Id,
                        follower_id = finalScore.Follower.Id,
                        score = finalScore.Placement
                    };

                    await SaveDataAsync(PG_FINAL_SCORES_TABLE_NAME, pgFinalScoreModel);
                }
            }

            await transaction.CommitAsync();
        }
        public async Task<ICompetition> GetCompetitionAsync(Guid id)
        {
            ICompetition competition = null;

            string query =
                "SELECT dance_conventions.id, dance_conventions.name, dance_conventions.date, competitions.division"
                + " FROM " + PG_COMPETITIONS_TABLE_NAME
                + " LEFT JOIN " + PG_DANCE_CONVENTIONS_TABLE_NAME + " ON dance_conventions.id = competitions.dance_convention_id"
                + " WHERE competitions.id = " + id;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid danceConventionId = reader.GetGuid(0);
                    string danceConventionName = reader.GetString(1);
                    string danceConventionDateString = reader.GetString(2);
                    string divisionString = reader.GetString(3);

                    DateTime date = DateTime.Parse(danceConventionDateString);
                    Division division = Util.GetDivisionFromString(divisionString);

                    competition = new ICompetition(danceConventionId, danceConventionName, date, division, id);
                }
            }

            List<IPrelimCompetition> prelimCompetitions = new();

            string prelimCompetitionQuery =
                "SELECT prelim_competitions.id, prelim_competitions.datetime, prelim_competitions.role, prelim_competitions.round"
                + " FROM " + PG_PRELIM_COMPETITIONS_TABLE_NAME
                + " WHERE prelim_competitions.competition_id = " + id;

            await using (var cmd = _dataSource.CreateCommand(prelimCompetitionQuery))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid prelimCompetitionId = reader.GetGuid(0);
                    string prelimCompetitionDateString = reader.GetString(1);
                    string prelimCompetitionRoleString = reader.GetString(2);
                    string prelimCompetitionRoundString = reader.GetString(3);

                    DateTime dateTime = DateTime.Parse(prelimCompetitionDateString);
                    Role role = Util.GetRoleFromString(prelimCompetitionRoleString);
                    Round round = Util.GetRoundFromString(prelimCompetitionRoundString);

                    prelimCompetitions.Add(new PrelimCompetition(
                        dateTime: dateTime,
                        division: competition.Division,
                        round: round,
                        role: role,
                        prelimScores: null,
                        competitors: null,
                        judges: null,
                        promotedCompetitors: null,
                        id: prelimCompetitionId));
                }
            }

            foreach (var prelimCompetition in prelimCompetitions)
            {
                List<PrelimScore> prelimScores = new();

                string prelimScoresQuery =
                    "SELECT id, prelim_competition_id, judge_id, competitor_id, callbackscore"
                    + " FROM prelim_scores"
                    + " WHERE prelim_competition_id = " + prelimCompetition.Id;

                await using (var cmd = _dataSource.CreateCommand(query))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid prelimScoreId = reader.GetGuid(0);
                        Guid prelimCompetitionId = reader.GetGuid(1);
                        Guid judgeId = reader.GetGuid(2);
                        Guid competitorId = reader.GetGuid(3);
                        string callbackScore = reader.GetString(3);

                        prelimScores.Add(new PrelimScore(
                            prelimCompetition: prelimCompetition,
                            judgeId: judgeId,
                            competitorId: competitorId,
                            callbackScore: Util.StringToCallbackScore(callbackScore),
                            id: prelimScoreId));
                    }
                }
            }


            return competition;
        }
        public async Task<IEnumerable<ICompetition>> GetAllCompetitionsAsync()
        {
        }
        public async Task DeleteCompetitionAsync(Guid id)
        {
        }
        public async Task DeleteAllCompetitionsAsync()
        {
        }

        public void Dispose()
        {
            _dataSource?.Dispose();
        }
    }
}
