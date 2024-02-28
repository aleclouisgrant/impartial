using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Impartial;
using ImpartialUI.Models;
using ImpartialUI.Models.PgModels;
using Impartial.Enums;
using System.Net;

namespace ImpartialUI.Services.DatabaseProvider
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

            _connectionString += ";INCLUDEERRORDETAIL=true";

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            dataSourceBuilder.MapEnum<Division>();
            dataSourceBuilder.MapEnum<Tier>();
            dataSourceBuilder.MapEnum<Round>();
            dataSourceBuilder.MapEnum<Role>();
            dataSourceBuilder.MapEnum<CallbackScore>();

            _dataSource = dataSourceBuilder.Build();
        }

        #region Helper
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
        private string CreateInsertQuery<U>(string table, U parameters)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = "INSERT INTO " + table;

            if (properties.Length == 0)
                return command + ";";

            command += " ";

            string columnNames = string.Empty;
            string parameterPositions = string.Empty;

            int startingPos = 0;
            while (startingPos < properties.Length)
            {
                if (properties[startingPos].GetValue(parameters) != null)
                {
                    columnNames += properties[startingPos].Name;
                    parameterPositions += "$1";
                    int count = 2;

                    for (int parameterIndex = startingPos + 1; parameterIndex < properties.Length; parameterIndex++)
                    {
                        if (properties[parameterIndex].GetValue(parameters) != null)
                        {
                            columnNames += ", " + properties[parameterIndex].Name;
                            parameterPositions += ", $" + count;
                            count++;
                        }
                    }

                    break;
                }

                startingPos++;
            }

            command += "(" + columnNames + ")";
            command += " VALUES (" + parameterPositions + ")";
            command += ";";

            return command;
        }
        private string CreateUpsertQuery<U>(string table, U parameters, string conflictParameter)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = "INSERT INTO " + table;

            if (properties.Length == 0)
                return command + ";";

            command += " ";

            string columnNames = string.Empty;
            string parameterPositions = string.Empty;
            string excluded = string.Empty;

            int startingPos = 0;
            while (startingPos < properties.Length)
            {
                if (properties[startingPos].GetValue(parameters) != null)
                {
                    columnNames += properties[startingPos].Name;
                    parameterPositions += "$1"; 
                    excluded += properties[startingPos].Name + " = EXCLUDED." + properties[startingPos].Name;

                    int count = 2;

                    for (int parameterIndex = startingPos + 1; parameterIndex < properties.Length; parameterIndex++)
                    {
                        if (properties[parameterIndex].GetValue(parameters) != null)
                        {
                            columnNames += ", " + properties[parameterIndex].Name;
                            parameterPositions += ", $" + count;
                            excluded += ", " + properties[parameterIndex].Name + " = EXCLUDED." + properties[parameterIndex].Name;

                            count++;
                        }
                    }

                    break;
                }

                startingPos++;
            }

            command += "(" + columnNames + ")";
            command += " VALUES (" + parameterPositions + ")";
            command += " ON CONFLICT (" + conflictParameter + ")";
            command += " DO UPDATE SET " + excluded;
            command += ";";

            return command;
        }
        private string CreateDeleteAllQuery<U>(string table)
        {
            return "DELETE FROM " + table + ";";
        }
        private string CreateDeleteWhereQuery<U>(string table, U parameters, string whereClause)
        {
            return "DELETE FROM " + table + "WHERE " + whereClause + ";";
        }

        private async Task SaveDataAsync<U>(string table, U parameters)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = CreateInsertQuery(table, parameters);

            var cmd = _dataSource.CreateCommand(command);

            foreach (PropertyInfo property in properties)
            {
                if (property.GetValue(parameters) != null)
                    cmd.Parameters.AddWithValue(property.GetValue(parameters));
            }
            await cmd.ExecuteNonQueryAsync();
        }
        private async Task UpsertDataAsync<U>(string table, U parameters, string conflictParameter)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();
            string command = CreateUpsertQuery(table, parameters, conflictParameter);

            await using (var cmd = _dataSource.CreateCommand(command))
            {
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(parameters) != null)
                        cmd.Parameters.AddWithValue(property.GetValue(parameters));
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
                id = competitor.CompetitorId,
                first_name = competitor.FirstName,
                last_name = competitor.LastName,
            };

            var competitorProfileModel = new PgCompetitorProfileModel
            {
                user_id = competitor.CompetitorId,
                wsdc_id = competitor.WsdcId
            };

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await UpsertDataAsync(PG_USERS_TABLE_NAME, userModel, "id");
            await UpsertDataAsync(PG_COMPETITOR_PROFILES_TABLE_NAME, competitorProfileModel, "user_id");

            await transaction.CommitAsync();
        }
        public async Task<ICompetitor?> GetCompetitorAsync(Guid id)
        {
            string query = "SELECT users.id, competitor_profiles.id, users.first_name, users.last_name, competitor_profiles.wsdc_id competitor_profiles.leader_rating competitor_profiles.follower_rating"
                + " FROM " + PG_USERS_TABLE_NAME + " INNER JOIN " + PG_COMPETITOR_PROFILES_TABLE_NAME + " ON users.id = competitor_profiles.user_id"
                + " WHERE competitor_profiles.id = " + id;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid usersId = reader.GetGuid(0);
                    Guid competitorId = reader.GetGuid(1);
                    string competitorFirstName = reader.GetString(2);
                    string competitorLastName = reader.GetString(3);
                    int competitorWsdcId = reader.GetInt32(4);
                    int leaderRating = reader.GetInt32(5);
                    int followerRating = reader.GetInt32(6);

                    return new Competitor(usersId, competitorId, competitorWsdcId, competitorFirstName, competitorLastName, leaderRating, 0, followerRating, 0);
                }
            }

            return null;
        }
        public async Task<ICompetitor?> GetCompetitorAsync(string firstName, string lastName)
        {
            string query = "SELECT users.id, competitor_profiles.id, users.first_name, users.last_name, competitor_profiles.wsdc_id competitor_profiles.leader_rating competitor_profiles.follower_rating"
               + " FROM " + PG_USERS_TABLE_NAME + " INNER JOIN " + PG_COMPETITOR_PROFILES_TABLE_NAME + " ON users.id = competitor_profiles.user_id"
               + " WHERE users.first_name = " + firstName + " AND users.last_name = " + lastName;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid userId = reader.GetGuid(0);
                    Guid competitorId = reader.GetGuid(1);
                    string competitorFirstName = reader.GetString(2);
                    string competitorLastName = reader.GetString(3);
                    int competitorWsdcId = reader.GetInt32(4);
                    int leaderRating = reader.GetInt32(5);
                    int followerRating = reader.GetInt32(6);

                    return new Competitor(userId, competitorId, competitorWsdcId, competitorFirstName, competitorLastName, leaderRating, 0, followerRating, 0);
                }
            }

            return null;
        }
        public async Task<IEnumerable<ICompetitor>> GetAllCompetitorsAsync()
        {
            string query = "SELECT users.id, competitor_profiles.id, users.first_name, users.last_name, competitor_profiles.wsdc_id, competitor_profiles.leader_rating, competitor_profiles.follower_rating"
            + " FROM " + PG_USERS_TABLE_NAME + " INNER JOIN " + PG_COMPETITOR_PROFILES_TABLE_NAME + " ON users.id = competitor_profiles.user_id";
            var results = new List<ICompetitor>();

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid userId = reader.GetGuid(0);
                    Guid competitorId = reader.GetGuid(1);
                    string competitorFirstName = reader.GetString(2);
                    string competitorLastName = reader.GetString(3);
                    int competitorWsdcId = reader.GetInt32(4);
                    int leaderRating = reader.GetInt32(5);
                    int followerRating = reader.GetInt32(6);

                    results.Add(new Competitor(userId, competitorId, competitorWsdcId, competitorFirstName, competitorLastName, leaderRating, 0, followerRating, 0));
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
                id = judge.JudgeId,
                first_name = judge.FirstName,
                last_name = judge.LastName,
            };

            var judgeModel = new PgJudgeProfileModel
            {
                id = Guid.NewGuid(),
                user_id = judge.JudgeId,
            };

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await UpsertDataAsync(PG_USERS_TABLE_NAME, userModel, "id");
            await UpsertDataAsync(PG_JUDGE_PROFILES_TABLE_NAME, judgeModel, "user_id");

            await transaction.CommitAsync();
        }
        public async Task<IJudge?> GetJudgeAsync(Guid id)
        {
            string query = "SELECT users.id, judge_profiles.id, users.first_name, users.last_name"
                + " FROM " + PG_USERS_TABLE_NAME + " INNER JOIN " + PG_JUDGE_PROFILES_TABLE_NAME + " ON users.id = judge_profiles.user_id"
                + " WHERE judge_profiles.id = " + id;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid userId = reader.GetGuid(0);
                    Guid judgeId = reader.GetGuid(1);
                    string judgeFirstName = reader.GetString(2);
                    string judgeLastName = reader.GetString(3);

                    return new Judge(userId, judgeId, judgeFirstName, judgeLastName);
                }
            }

            return null;
        }
        public async Task<IJudge?> GetJudgeAsync(string firstName, string lastName)
        {
            string query = "SELECT users.id, judge_profiles.id, users.first_name, users.last_name"
                + " FROM " + PG_USERS_TABLE_NAME + " INNER JOIN " + PG_JUDGE_PROFILES_TABLE_NAME + " ON users.id = judge_profiles.user_id"
               + " WHERE users.first_name = " + firstName + " AND users.last_name = " + lastName;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid userId = reader.GetGuid(0);
                    Guid judgeId = reader.GetGuid(1);
                    string judgeFirstName = reader.GetString(2);
                    string judgeLastName = reader.GetString(3);

                    return new Judge(userId, judgeId, judgeFirstName, judgeLastName);
                }
            }

            return null;
        }
        public async Task<IEnumerable<IJudge>> GetAllJudgesAsync()
        {
            string query = "SELECT users.id, judge_profiles.id, users.first_name, users.last_name"
                + " FROM " + PG_USERS_TABLE_NAME + " INNER JOIN " + PG_JUDGE_PROFILES_TABLE_NAME + " ON users.id = judge_profiles.user_id";

            var results = new List<IJudge>();

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid userId = reader.GetGuid(0);
                    Guid judgeId = reader.GetGuid(1);
                    string judgeFirstName = reader.GetString(2);
                    string judgeLastName = reader.GetString(3);

                    results.Add(new Judge(userId, judgeId, judgeFirstName, judgeLastName));
                }
            }

            return results;
        }
        public async Task DeleteJudgeAsync(Guid id)
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

            await UpsertDataAsync(PG_DANCE_CONVENTIONS_TABLE_NAME, pgDanceConventionModel, "id");
        }
        public async Task<IDanceConvention?> GetDanceConventionAsync(Guid id)
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

                    return new DanceConvention(danceConventionName, DateTime.Parse(danceConventionDateString), danceConventionId);
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
                    DateTime danceConventionDateString = reader.GetDateTime(2);

                    results.Add(new DanceConvention(danceConventionName, danceConventionDateString, danceConventionId));
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
            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var pgCompetitionModel = new PgCompetitionModel
            {
                id = competition.Id,
                dance_convention_id = danceConventionId,
                division = competition.Division,
                leader_tier = competition.LeaderTier,
                follower_tier = competition.FollowerTier
            };

            await SaveDataAsync(PG_COMPETITIONS_TABLE_NAME, pgCompetitionModel);

            var uploadedCompetitorRegistrations = new List<PgCompetitorRegistrationModel>();
            var uploadedCompetitorRecords = new List<PgCompetitorRecordModel>();

            foreach (var pairedPrelimCompetition in competition.PairedPrelimCompetitions)
            {
                var prelimScores = new List<PgPrelimScoreModel>();
                var prelimCompetitorRegistrations = new List<PgCompetitorRegistrationModel>();
                var prelimCompetitorRecords = new List<PgCompetitorRecordModel>();
                
                var leaderPromotedCompetitors = new List<PgPromotedCompetitorModel>();
                var followerPromotedCompetitors = new List<PgPromotedCompetitorModel>();

                var leaderPrelimCompetitionModel = new PgPrelimCompetitionModel
                {
                    competition_id = competition.Id,
                    date_time = pairedPrelimCompetition.LeaderPrelimCompetition.DateTime,
                    role = Role.Leader,
                    round = pairedPrelimCompetition.LeaderPrelimCompetition.Round,
                    alternate1_id = pairedPrelimCompetition.LeaderPrelimCompetition.Alternate1.CompetitorId,
                    alternate2_id = pairedPrelimCompetition.LeaderPrelimCompetition.Alternate2.CompetitorId,
                };

                var followerPrelimCompetitionModel = new PgPrelimCompetitionModel
                {
                    competition_id = competition.Id,
                    date_time = pairedPrelimCompetition.FollowerPrelimCompetition.DateTime,
                    role = Role.Follower,
                    round = pairedPrelimCompetition.FollowerPrelimCompetition.Round,
                    alternate1_id = pairedPrelimCompetition.FollowerPrelimCompetition.Alternate1.CompetitorId,
                    alternate2_id = pairedPrelimCompetition.FollowerPrelimCompetition.Alternate2.CompetitorId,
                };

                foreach (var competitor in pairedPrelimCompetition.LeaderPrelimCompetition.Competitors)
                {
                    if (!prelimCompetitorRegistrations.Any(reg => reg.competitor_profile_id == competitor.CompetitorId))
                    {
                        prelimCompetitorRegistrations.Add(new PgCompetitorRegistrationModel
                        {
                            id = Guid.NewGuid(),
                            competitor_profile_id = competitor.CompetitorId,
                            dance_convention_id = danceConventionId,
                        });
                    }

                    if (!prelimCompetitorRecords.Any(rec => rec.competitor_registration_id == competitor.CompetitorId)
                        && !uploadedCompetitorRecords.Any(rec => rec.competitor_registration_id == competitor.CompetitorId)
                        && (bool)!competition.FinalCompetition?.Couples.Any(c => c.Leader.CompetitorId == competitor.CompetitorId))
                    {
                        prelimCompetitorRecords.Add(new PgCompetitorRecordModel
                        {
                            competition_id = competition.Id,
                            competitor_registration_id = prelimCompetitorRegistrations.First(reg => reg.competitor_profile_id == competitor.CompetitorId).id,
                        });
                    }
                }

                foreach (var promotedCompetitor in pairedPrelimCompetition.LeaderPrelimCompetition.PromotedCompetitors)
                {
                    leaderPromotedCompetitors.Add(new PgPromotedCompetitorModel
                    {
                        prelim_competition_id = leaderPrelimCompetitionModel.id,
                        competitor_id = promotedCompetitor.CompetitorId
                    });
                }

                foreach (var competitor in pairedPrelimCompetition.FollowerPrelimCompetition.Competitors)
                {
                    if (!prelimCompetitorRegistrations.Any(reg => reg.competitor_profile_id == competitor.CompetitorId))
                    {
                        prelimCompetitorRegistrations.Add(new PgCompetitorRegistrationModel
                        {
                            id = Guid.NewGuid(),
                            competitor_profile_id = competitor.CompetitorId,
                            dance_convention_id = danceConventionId,
                        });
                    }

                    if (!prelimCompetitorRecords.Any(rec => rec.competitor_registration_id == competitor.CompetitorId)
                        && !uploadedCompetitorRecords.Any(rec => rec.competitor_registration_id == competitor.CompetitorId)
                        && (bool)!competition.FinalCompetition?.Couples.Any(c => c.Follower.CompetitorId == competitor.CompetitorId))
                    {
                        prelimCompetitorRecords.Add(new PgCompetitorRecordModel
                        {
                            competition_id = competition.Id,
                            competitor_registration_id = prelimCompetitorRegistrations.First(reg => reg.competitor_profile_id == competitor.CompetitorId).id,
                        });
                    }
                }

                foreach (var promotedCompetitor in pairedPrelimCompetition.FollowerPrelimCompetition.PromotedCompetitors)
                {
                    followerPromotedCompetitors.Add(new PgPromotedCompetitorModel
                    {
                        prelim_competition_id = followerPrelimCompetitionModel.id,
                        competitor_id = promotedCompetitor.CompetitorId
                    });
                }

                foreach (var prelimScore in pairedPrelimCompetition.LeaderPrelimCompetition.PrelimScores)
                {
                    prelimScores.Add(new PgPrelimScoreModel
                    {
                        id = prelimScore.Id,
                        prelim_competition_id = pairedPrelimCompetition.LeaderPrelimCompetition.Id,
                        competitor_id = prelimScore.Competitor.CompetitorId,
                        judge_id = prelimScore.Judge.JudgeId,
                        callback_score = prelimScore.CallbackScore,
                    });
                }

                foreach (var prelimScore in pairedPrelimCompetition.FollowerPrelimCompetition.PrelimScores)
                {
                    prelimScores.Add(new PgPrelimScoreModel
                    {
                        id = prelimScore.Id,
                        prelim_competition_id = pairedPrelimCompetition.FollowerPrelimCompetition.Id,
                        competitor_id = prelimScore.Competitor.CompetitorId,
                        judge_id = prelimScore.Judge.JudgeId,
                        callback_score = prelimScore.CallbackScore,
                    });
                }

                // Save all data
                foreach (var competitorRegistration in prelimCompetitorRegistrations)
                {
                    await SaveDataAsync(PG_COMPETITOR_REGISTRATIONS_TABLE_NAME, competitorRegistration);
                }

                foreach (var competitorRecord in prelimCompetitorRecords)
                {
                    await SaveDataAsync(PG_COMPETITOR_RECORDS_TABLE_NAME, competitorRecord);
                }

                await SaveDataAsync(PG_PRELIM_COMPETITIONS_TABLE_NAME, leaderPrelimCompetitionModel);
                await SaveDataAsync(PG_PRELIM_COMPETITIONS_TABLE_NAME, followerPrelimCompetitionModel);
                
                foreach (var leaderPromotedCompetitor in leaderPromotedCompetitors)
                {
                    await SaveDataAsync(PG_PROMOTED_COMPETITORS_TABLE_NAME, leaderPromotedCompetitor);
                }

                foreach (var followerPromotedCompetitor in followerPromotedCompetitors)
                {
                    await SaveDataAsync(PG_PROMOTED_COMPETITORS_TABLE_NAME, followerPromotedCompetitor);
                }

                foreach (var prelimScore in prelimScores)
                {
                    await SaveDataAsync(PG_PRELIM_SCORES_TABLE_NAME, prelimScore);
                }

                uploadedCompetitorRegistrations.AddRange(prelimCompetitorRegistrations);
                uploadedCompetitorRecords.AddRange(prelimCompetitorRecords);
            }

            if (competition.FinalCompetition != null)
            {
                var finalCompetitorRegistrations = new List<PgCompetitorRegistrationModel>();
                var finalCompetitorRecords = new List<PgCompetitorRecordModel>();

                var placements = new List<PgPlacementModel>();
                var finalScores = new List<PgFinalScoreModel>();

                var finalCompetitionModel = new PgFinalCompetitionModel
                {
                    id = competition.FinalCompetition.Id,
                    competition_id = competition.Id,
                    datetime = competition.FinalCompetition.DateTime
                };

                foreach (var couple in competition.FinalCompetition.Couples)
                {
                    placements.Add(new PgPlacementModel
                    {
                        id = Guid.NewGuid(),
                        final_competition_id = competition.FinalCompetition.Id,
                        leader_id = couple.Leader.CompetitorId,
                        follower_id = couple.Follower.CompetitorId,
                        placement = couple.Placement
                    });

                    // in case we're only uploading a final competition, need to add registrations and records
                    if (!uploadedCompetitorRegistrations.Where(reg => reg.competitor_profile_id == couple.Leader.CompetitorId).Any()
                        && !finalCompetitorRegistrations.Where(reg => reg.competitor_profile_id == couple.Leader.CompetitorId).Any())
                    {
                        finalCompetitorRegistrations.Add(new PgCompetitorRegistrationModel
                        {
                            id = Guid.NewGuid(),
                            competitor_profile_id = couple.Leader.CompetitorId,
                            dance_convention_id = danceConventionId,
                        });
                    }

                    if (!uploadedCompetitorRegistrations.Where(reg => reg.competitor_profile_id == couple.Follower.CompetitorId).Any()
                        && !finalCompetitorRegistrations.Where(reg => reg.competitor_profile_id == couple.Follower.CompetitorId).Any())
                    {
                        finalCompetitorRegistrations.Add(new PgCompetitorRegistrationModel
                        {
                            id = Guid.NewGuid(),
                            competitor_profile_id = couple.Follower.CompetitorId,
                            dance_convention_id = danceConventionId,
                        });
                    }

                    var leaderRecord = finalCompetitorRecords.Where(rec => rec.competitor_registration_id == couple.Leader.CompetitorId).FirstOrDefault();
                    if (leaderRecord == null)
                    {
                        var registrationId =
                            uploadedCompetitorRegistrations.FirstOrDefault(reg => reg.competitor_profile_id == couple.Leader.CompetitorId)?.id ??
                            finalCompetitorRegistrations.First(reg => reg.competitor_profile_id == couple.Leader.CompetitorId).id;

                        finalCompetitorRecords.Add(new PgCompetitorRecordModel
                        {
                            competition_id = competition.Id,
                            competitor_registration_id = registrationId,
                            placement = couple.Placement,
                            points_earned = Util.GetAwardedPoints(competition.LeaderTier, couple.Placement, competition.Date)
                        });
                    }

                    var followerRecord = finalCompetitorRecords.Where(rec => rec.competitor_registration_id == couple.Leader.CompetitorId).FirstOrDefault();
                    if (followerRecord == null)
                    {
                        var registrationId = 
                            uploadedCompetitorRegistrations.FirstOrDefault(reg => reg.competitor_profile_id == couple.Leader.CompetitorId)?.id ??
                            finalCompetitorRegistrations.First(reg => reg.competitor_profile_id == couple.Leader.CompetitorId).id;

                        finalCompetitorRecords.Add(new PgCompetitorRecordModel
                        {
                            competition_id = competition.Id,
                            competitor_registration_id = registrationId,
                            placement = couple.Placement,
                            points_earned = Util.GetAwardedPoints(competition.FollowerTier, couple.Placement, competition.Date)
                        });
                    }
                }

                foreach (var finalScore in competition.FinalCompetition.FinalScores)
                {
                    finalScores.Add(new PgFinalScoreModel
                    {
                        id = finalScore.Id,
                        final_competition_id = competition.FinalCompetition.Id,
                        judge_id = finalScore.Judge.JudgeId,
                        leader_id = finalScore.Leader.CompetitorId,
                        follower_id = finalScore.Follower.CompetitorId,
                        score = finalScore.Score
                    });
                }

                // Sava all data
                foreach (var registration in finalCompetitorRegistrations)
                {
                    await SaveDataAsync(PG_COMPETITOR_REGISTRATIONS_TABLE_NAME, registration);
                }

                foreach (var record in finalCompetitorRecords)
                {
                    await SaveDataAsync(PG_COMPETITOR_RECORDS_TABLE_NAME, record);
                }

                await SaveDataAsync(PG_FINAL_COMPETITIONS_TABLE_NAME, finalCompetitionModel);

                foreach (var placementModel in placements)
                {
                    await SaveDataAsync(PG_PLACEMENTS_TABLE_NAME, placementModel);
                }

                foreach (var finalScore in finalScores)
                {
                    await SaveDataAsync(PG_FINAL_SCORES_TABLE_NAME, finalScore);
                }
            }

            await transaction.CommitAsync();
        }
        public async Task<ICompetition?> GetCompetitionAsync(Guid id)
        {
            ICompetition competition = null;

            string query =
                "SELECT dance_conventions.id, dance_conventions.name, dance_conventions.date, competitions.division"
                + " FROM " + PG_COMPETITIONS_TABLE_NAME
                + " INNER JOIN " + PG_DANCE_CONVENTIONS_TABLE_NAME + " ON dance_conventions.id = competitions.dance_convention_id"
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

                    competition = new Competition(danceConventionId, danceConventionName, date, division, id);
                }
            }

            List<IPrelimCompetition> prelimCompetitions = new();

            string prelimCompetitionQuery =
                "SELECT id, datetime, role, round, alternate_1_id, alternate_2_id"
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
                    Guid alternate1Id = reader.GetGuid(4);
                    Guid alternate2Id = reader.GetGuid(5);

                    DateTime dateTime = DateTime.Parse(prelimCompetitionDateString);
                    Role role = Util.GetRoleFromString(prelimCompetitionRoleString);
                    Round round = Util.GetRoundFromString(prelimCompetitionRoundString);

                    prelimCompetitions.Add(new PrelimCompetition(
                        dateTime: dateTime,
                        division: competition.Division,
                        round: round,
                        role: role,
                        prelimScores: null,
                        promotedCompetitors: null,
                        alternate1: App.CompetitorsDb.Where(c => c.CompetitorId == alternate1Id).First(),
                        alternate2: App.CompetitorsDb.Where(c => c.CompetitorId == alternate2Id).First(),
                        id: prelimCompetitionId));
                }
            }

            foreach (var prelimCompetition in prelimCompetitions)
            {
                List<IPrelimScore> prelimScores = new();

                string prelimScoresQuery =
                    "SELECT id, judge_id, competitor_id, callbackscore"
                    + " FROM prelim_scores"
                    + " WHERE prelim_competition_id = " + prelimCompetition.Id;

                await using (var cmd = _dataSource.CreateCommand(prelimScoresQuery))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid prelimScoreId = reader.GetGuid(0);
                        Guid judgeId = reader.GetGuid(1);
                        Guid competitorId = reader.GetGuid(2);
                        string callbackScore = reader.GetString(3);

                        prelimScores.Add(new PrelimScore(
                            judgeId: judgeId,
                            competitorId: competitorId,
                            callbackScore: Util.StringToCallbackScore(callbackScore),
                            id: prelimScoreId));
                    }
                }

                List<ICompetitor> promotedCompetitors = new();

                string promotedCompetitorsQuery =
                    "SELECT competitor_id"
                    + " FROM promoted_competitors"
                    + " WHERE prelim_competition_id = " + prelimCompetition.Id;

                await using (var cmd = _dataSource.CreateCommand(promotedCompetitorsQuery))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid competitorId = reader.GetGuid(0);

                        promotedCompetitors.Add(App.CompetitorsDb.Find(c => c.CompetitorId == competitorId));
                    }
                }

                prelimCompetition.PrelimScores = prelimScores;
                prelimCompetition.PromotedCompetitors = promotedCompetitors;
            }

            List<IPairedPrelimCompetition> pairedPrelimCompetitions = new();
            List<Round> rounds = new();
            foreach (var prelimCompetition in prelimCompetitions)
            {
                if (!rounds.Contains(prelimCompetition.Round))
                {
                    rounds.Add(prelimCompetition.Round);
                }
            }

            foreach (var round in rounds)
            {
                pairedPrelimCompetitions.Add(new PairedPrelimCompetition(
                    round,
                    prelimCompetitions.Where(p => p.Round == round && p.Role == Role.Leader).FirstOrDefault(),
                    prelimCompetitions.Where(p => p.Round == round && p.Role == Role.Follower).FirstOrDefault()));
            }

            IFinalCompetition? finalCompetition = null;

            string finalCompetitionQuery =
                "SELECT id, datetime"
                + " FROM " + PG_FINAL_COMPETITIONS_TABLE_NAME
                + " WHERE competition_id = " + id;

            await using (var cmd = _dataSource.CreateCommand(finalCompetitionQuery))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid finalCompetitionId = reader.GetGuid(0);
                    string finalCompetitionDateString = reader.GetString(1);

                    DateTime dateTime = DateTime.Parse(finalCompetitionDateString);

                    finalCompetition = new FinalCompetition(
                        dateTime: dateTime,
                        division: competition.Division,
                        finalScores: null,
                        id: finalCompetitionId);
                }
            }

            if (finalCompetition != null)
            {
                List<ICouple> couples = new();

                string placementsQuery =
                    "SELECT leader_id, follower_id, score"
                    + " FROM placements"
                    + " WHERE final_competition_id = " + finalCompetition.Id;

                await using (var cmd = _dataSource.CreateCommand(placementsQuery))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid leaderId = reader.GetGuid(0);
                        Guid followerId = reader.GetGuid(1);
                        int placement = reader.GetInt32(2);

                        couples.Add(new Couple(
                            leader: App.CompetitorsDb.Where(c => c.CompetitorId == leaderId).FirstOrDefault(),
                            follower: App.CompetitorsDb.Where(c => c.CompetitorId == followerId).FirstOrDefault(),
                            placement: placement));
                    }
                }

                List<IFinalScore> finalScores = new();

                string finalScoresQuery =
                    "SELECT id, judge_id, leader_id, follower_id, score"
                    + " FROM final_scores"
                    + " WHERE final_competition_id = " + finalCompetition.Id;

                await using (var cmd = _dataSource.CreateCommand(finalScoresQuery))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid finalScoreId = reader.GetGuid(0);
                        Guid judgeId = reader.GetGuid(1);
                        Guid leaderId = reader.GetGuid(2);
                        Guid followerId = reader.GetGuid(3);
                        int score = reader.GetInt32(4);
                        int placement = couples.Where(c => c.Leader.CompetitorId == leaderId && c.Follower.CompetitorId == followerId).FirstOrDefault().Placement;

                        finalScores.Add(new FinalScore(
                            judgeId: judgeId,
                            leaderId: leaderId,
                            followerId: followerId,
                            score: score,
                            placement: placement,
                            id: finalScoreId));
                    }
                }

                finalCompetition.FinalScores = finalScores;
            }

            competition.FinalCompetition = finalCompetition;
            competition.PairedPrelimCompetitions = pairedPrelimCompetitions;

            return competition;
        }
        public async Task<IEnumerable<ICompetition>> GetAllCompetitionsAsync()
        {
            var competitions = new List<ICompetition>();

            var competitionIds = new List<Guid>();

            string query =
                "SELECT id"
                + " FROM " + PG_COMPETITIONS_TABLE_NAME;

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid competitionId = reader.GetGuid(0);
                    competitionIds.Add(competitionId);
                }
            }

            foreach (var competitionId in competitionIds)
            {
                competitions.Add(await GetCompetitionAsync(competitionId));
            }

            return competitions;
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