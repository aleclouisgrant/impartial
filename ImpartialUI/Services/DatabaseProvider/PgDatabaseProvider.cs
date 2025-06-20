﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Impartial;
using ImpartialUI.Models;
using ImpartialUI.Models.PgModels;
using Impartial.Enums;
using Impartial.PgTableSchema;
using System.Net.Security;

namespace ImpartialUI.Services.DatabaseProvider
{
    public class PgDatabaseProvider : IDatabaseProvider, IDisposable
    {
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
        private string CreateSelectQuery<T>(string table, T parameters)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

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
        private string CreateInsertQuery<T>(string table, T parameters)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
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
        private string CreateUpsertQuery<T>(string table, T parameters, string conflictParameter)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
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
        private string CreateDeleteAllQuery(string table)
        {
            return "DELETE FROM " + table + ";";
        }
        private string CreateDeleteWhereQuery<T>(string table, string parameter, T value)
        {
            return "DELETE FROM " + table + " WHERE " + parameter + " = " + value.ToString() + ";";
        }

        private async Task SaveDataAsync<T>(string table, T parameters)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            string command = CreateInsertQuery(table, parameters);

            var cmd = _dataSource.CreateCommand(command);

            foreach (PropertyInfo property in properties)
            {
                if (property.GetValue(parameters) != null)
                    cmd.Parameters.AddWithValue(property.GetValue(parameters));
            }
            await cmd.ExecuteNonQueryAsync();
        }
        private async Task UpsertDataAsync<T>(string table, T parameters, string conflictParameter)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
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
        
        private async Task DeleteAllDataAsync(string table)
        {
            string command = CreateDeleteAllQuery(table);

            await using (var cmd = _dataSource.CreateCommand(command))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }
        private async Task DeleteDataAsync<T>(string table, string parameter, T value)
        {
            string command = CreateDeleteWhereQuery(table, parameter, value);
            await using (var cmd = _dataSource.CreateCommand(command))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }
        #endregion

        public async Task UpsertCompetitorAsync(ICompetitor competitor)
        {
            var userModel = new PgUserModel
            {
                id = competitor.UserId,
                first_name = competitor.FirstName,
                last_name = competitor.LastName,
            };

            var competitorProfileModel = new PgCompetitorProfileModel
            {
                id = competitor.CompetitorId,
                user_id = competitor.UserId,
                wsdc_id = competitor.WsdcId
            };

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await UpsertDataAsync(PgUsersTableSchema.TableName, userModel, nameof(PgUserModel.id));
            await UpsertDataAsync(PgCompetitorProfilesTableSchema.TableName, competitorProfileModel, nameof(PgCompetitorProfileModel.user_id));

            await transaction.CommitAsync();
        }
        public async Task<ICompetitor?> GetCompetitorAsync(Guid id)
        {
            string query = "SELECT users.id, competitor_profiles.id, users.first_name, users.last_name, competitor_profiles.wsdc_id competitor_profiles.leader_rating competitor_profiles.follower_rating"
                + " FROM " + PgUsersTableSchema.TableName + " INNER JOIN " + PgCompetitorProfilesTableSchema.TableName + " ON users.id = competitor_profiles.user_id"
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
               + " FROM " + PgUsersTableSchema.TableName + " INNER JOIN " + PgCompetitorProfilesTableSchema.TableName + " ON users.id = competitor_profiles.user_id"
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
            + " FROM " + PgUsersTableSchema.TableName + " INNER JOIN " + PgCompetitorProfilesTableSchema.TableName + " ON users.id = competitor_profiles.user_id";
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
            await DeleteDataAsync(PgCompetitorProfilesTableSchema.TableName, nameof(PgCompetitorProfileModel.id), id);
        }
        public async Task DeleteAllCompetitorsAsync()
        {
            await DeleteAllDataAsync(PgCompetitorProfilesTableSchema.TableName);
        }

        public async Task UpsertJudgeAsync(IJudge judge)
        {
            var userModel = new PgUserModel
            {
                id = judge.UserId,
                first_name = judge.FirstName,
                last_name = judge.LastName,
            };

            var judgeModel = new PgJudgeProfileModel
            {
                id = judge.JudgeId,
                user_id = judge.UserId,
            };

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            await UpsertDataAsync(PgUsersTableSchema.TableName, userModel, nameof(PgUserModel.id));
            await UpsertDataAsync(PgJudgeProfilesTableSchema.TableName, judgeModel, nameof(PgJudgeProfileModel.user_id));

            await transaction.CommitAsync();
        }
        public async Task<IJudge?> GetJudgeAsync(Guid id)
        {
            string query = "SELECT users.id, judge_profiles.id, users.first_name, users.last_name"
                + " FROM " + PgUsersTableSchema.TableName + " INNER JOIN " + PgJudgeProfilesTableSchema.TableName + " ON users.id = judge_profiles.user_id"
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
                + " FROM " + PgUsersTableSchema.TableName + " INNER JOIN " + PgJudgeProfilesTableSchema.TableName + " ON users.id = judge_profiles.user_id"
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
                + " FROM " + PgUsersTableSchema.TableName + " INNER JOIN " + PgJudgeProfilesTableSchema.TableName + " ON users.id = judge_profiles.user_id";

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
            await DeleteDataAsync(PgJudgeProfilesTableSchema.TableName, nameof(PgJudgeProfileModel.id), id);
        }
        public async Task DeleteAllJudgesAsync()
        {
            await DeleteAllDataAsync(PgJudgeProfilesTableSchema.TableName);
        }

        public async Task UpsertDanceConventionAsync(IDanceConvention convention)
        {
            var pgDanceConventionModel = new PgDanceConventionModel
            {
                id = convention.Id,
                name = convention.Name,
                date = convention.Date,
            };

            await UpsertDataAsync(PgDanceConventionsTableSchema.TableName, pgDanceConventionModel, nameof(PgDanceConventionModel.id));
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
            await DeleteDataAsync(PgDanceConventionsTableSchema.TableName, nameof(PgDanceConventionModel.id), id);
        }
        public async Task DeleteAllDanceConventionsAsync()
        {
            await DeleteAllDataAsync(PgDanceConventionsTableSchema.TableName);
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

            await SaveDataAsync(PgCompetitionsTableSchema.TableName, pgCompetitionModel);

            var uploadedCompetitorRegistrations = new List<PgCompetitorRegistrationModel>();
            var uploadedCompetitorRecords = new List<PgCompetitorRecordModel>();

            foreach (var pairedPrelimCompetition in competition.PairedPrelimCompetitions)
            {
                var prelimScores = new List<PgPrelimScoreModel>();
                var prelimCompetitorRegistrations = new List<PgCompetitorRegistrationModel>();
                var prelimCompetitorRecords = new List<PgCompetitorRecordModel>();
                
                var leaderPromotedCompetitors = new List<PgPromotedCompetitorModel>();
                var followerPromotedCompetitors = new List<PgPromotedCompetitorModel>();

                PgPrelimCompetitionModel leaderPrelimCompetitionModel = null;
                PgPrelimCompetitionModel followerPrelimCompetitionModel = null;

                if (pairedPrelimCompetition.LeaderPrelimCompetition != null)
                {
                    leaderPrelimCompetitionModel = new PgPrelimCompetitionModel
                    {
                        id = pairedPrelimCompetition.LeaderPrelimCompetition.Id,
                        competition_id = competition.Id,
                        date_time = pairedPrelimCompetition.LeaderPrelimCompetition.DateTime,
                        role = Role.Leader,
                        round = pairedPrelimCompetition.LeaderPrelimCompetition.Round,
                        alternate1_id = pairedPrelimCompetition.LeaderPrelimCompetition.Alternate1?.CompetitorId,
                        alternate2_id = pairedPrelimCompetition.LeaderPrelimCompetition.Alternate2?.CompetitorId,
                    };

                    foreach (var competitor in pairedPrelimCompetition.LeaderPrelimCompetition.Competitors)
                    {
                        if (!prelimCompetitorRegistrations.Any(reg => reg.competitor_profile_id == competitor.CompetitorId)
                            && !uploadedCompetitorRegistrations.Any(reg => reg.competitor_profile_id == competitor.CompetitorId))
                        {
                            prelimCompetitorRegistrations.Add(new PgCompetitorRegistrationModel
                            {
                                id = Guid.NewGuid(),
                                competitor_profile_id = competitor.CompetitorId,
                                dance_convention_id = danceConventionId,
                                bib_number = pairedPrelimCompetition.LeaderPrelimCompetition.CompetitorRegistrations.FirstOrDefault(c => c.Competitor == competitor)?.BibNumber
                            });
                        }

                        bool pendingRegistration = prelimCompetitorRegistrations.Any(reg => reg.competitor_profile_id == competitor.CompetitorId);
                        bool recordAlreadyUploaded = pendingRegistration ?
                            uploadedCompetitorRecords.Any(rec => rec.competitor_registration_id == prelimCompetitorRegistrations.FirstOrDefault(reg => reg.competitor_profile_id == competitor.CompetitorId)?.id)
                            : uploadedCompetitorRecords.Any(rec => rec.competitor_registration_id == uploadedCompetitorRegistrations.FirstOrDefault(reg => reg.competitor_profile_id == competitor.CompetitorId)?.id);
                        bool alreadyInRecords = prelimCompetitorRecords.Any(rec => rec.competitor_registration_id == competitor.CompetitorId);
                        bool willBeUploadedDuringFinals = (bool)competition.FinalCompetition?.Couples.Any(c => c.Leader.CompetitorId == competitor.CompetitorId);

                        if (!(recordAlreadyUploaded || alreadyInRecords || willBeUploadedDuringFinals))
                        {
                            if (pendingRegistration)
                            {
                                prelimCompetitorRecords.Add(new PgCompetitorRecordModel
                                {
                                    competition_id = competition.Id,
                                    competitor_registration_id = prelimCompetitorRegistrations.First(reg => reg.competitor_profile_id == competitor.CompetitorId).id,
                                });
                            }
                            else
                            {
                                prelimCompetitorRecords.Add(new PgCompetitorRecordModel
                                {
                                    competition_id = competition.Id,
                                    competitor_registration_id = uploadedCompetitorRegistrations.First(reg => reg.competitor_profile_id == competitor.CompetitorId).id,
                                });
                            }
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

                    foreach (var prelimScore in pairedPrelimCompetition.LeaderPrelimCompetition.PrelimScores)
                    {
                        prelimScores.Add(new PgPrelimScoreModel
                        {
                            id = prelimScore.Id,
                            prelim_competition_id = pairedPrelimCompetition.LeaderPrelimCompetition.Id,
                            competitor_id = prelimScore.Competitor?.CompetitorId,
                            judge_id = prelimScore.Judge?.JudgeId,
                            callback_score = prelimScore.CallbackScore,
                        });
                    }
                }

                if (pairedPrelimCompetition.FollowerPrelimCompetition != null) 
                { 
                    followerPrelimCompetitionModel = new PgPrelimCompetitionModel
                    {
                        id = pairedPrelimCompetition.FollowerPrelimCompetition.Id,
                        competition_id = competition.Id,
                        date_time = pairedPrelimCompetition.FollowerPrelimCompetition.DateTime,
                        role = Role.Follower,
                        round = pairedPrelimCompetition.FollowerPrelimCompetition.Round,
                        alternate1_id = pairedPrelimCompetition.FollowerPrelimCompetition.Alternate1?.CompetitorId,
                        alternate2_id = pairedPrelimCompetition.FollowerPrelimCompetition.Alternate2?.CompetitorId,
                    };

                    foreach (var competitor in pairedPrelimCompetition.FollowerPrelimCompetition.Competitors)
                    {
                        if (!prelimCompetitorRegistrations.Any(reg => reg.competitor_profile_id == competitor.CompetitorId)
                            && !uploadedCompetitorRegistrations.Any(reg => reg.competitor_profile_id == competitor.CompetitorId))
                        {
                            prelimCompetitorRegistrations.Add(new PgCompetitorRegistrationModel
                            {
                                id = Guid.NewGuid(),
                                competitor_profile_id = competitor.CompetitorId,
                                dance_convention_id = danceConventionId,
                                bib_number = pairedPrelimCompetition.FollowerPrelimCompetition.CompetitorRegistrations.FirstOrDefault(c => c.Competitor == competitor)?.BibNumber
                            });
                        }

                        bool pendingRegistration = prelimCompetitorRegistrations.Any(reg => reg.competitor_profile_id == competitor.CompetitorId);
                        bool recordAlreadyUploaded = pendingRegistration ?
                            uploadedCompetitorRecords.Any(rec => rec.competitor_registration_id == prelimCompetitorRegistrations.FirstOrDefault(reg => reg.competitor_profile_id == competitor.CompetitorId)?.id)
                            : uploadedCompetitorRecords.Any(rec => rec.competitor_registration_id == uploadedCompetitorRegistrations.FirstOrDefault(reg => reg.competitor_profile_id == competitor.CompetitorId)?.id);
                        bool alreadyInRecords = prelimCompetitorRecords.Any(rec => rec.competitor_registration_id == competitor.CompetitorId);
                        bool willBeUploadedDuringFinals = (bool)competition.FinalCompetition?.Couples.Any(c => c.Follower.CompetitorId == competitor.CompetitorId);

                        if (!(recordAlreadyUploaded || alreadyInRecords || willBeUploadedDuringFinals))
                        {
                            if (pendingRegistration)
                            {
                                prelimCompetitorRecords.Add(new PgCompetitorRecordModel
                                {
                                    competition_id = competition.Id,
                                    competitor_registration_id = prelimCompetitorRegistrations.First(reg => reg.competitor_profile_id == competitor.CompetitorId).id,
                                });
                            }
                            else
                            {
                                prelimCompetitorRecords.Add(new PgCompetitorRecordModel
                                {
                                    competition_id = competition.Id,
                                    competitor_registration_id = uploadedCompetitorRegistrations.First(reg => reg.competitor_profile_id == competitor.CompetitorId).id,
                                });
                            }
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

                    foreach (var prelimScore in pairedPrelimCompetition.FollowerPrelimCompetition.PrelimScores)
                    {
                        prelimScores.Add(new PgPrelimScoreModel
                        {
                            id = prelimScore.Id,
                            prelim_competition_id = pairedPrelimCompetition.FollowerPrelimCompetition.Id,
                            competitor_id = prelimScore.Competitor?.CompetitorId,
                            judge_id = prelimScore.Judge?.JudgeId,
                            callback_score = prelimScore.CallbackScore,
                        });
                    }
                }

                // Save all data
                foreach (var competitorRegistration in prelimCompetitorRegistrations)
                {
                    await SaveDataAsync(PgCompetitorRegistrationsTableSchema.TableName, competitorRegistration);
                }

                foreach (var competitorRecord in prelimCompetitorRecords)
                {
                    await SaveDataAsync(PgCompetitorRecordsTableSchema.TableName, competitorRecord);
                }

                if (pairedPrelimCompetition.LeaderPrelimCompetition != null)
                {
                    await SaveDataAsync(PgPrelimCompetitionsTableSchema.TableName, leaderPrelimCompetitionModel);

                    foreach (var leaderPromotedCompetitor in leaderPromotedCompetitors)
                    {
                        await SaveDataAsync(PgPromotedCompetitorsTableSchema.TableName, leaderPromotedCompetitor);
                    }

                    foreach (var judge in pairedPrelimCompetition.LeaderPrelimCompetition.Judges)
                    {
                        await UpsertJudgeAsync(judge);
                    }
                }

                if (pairedPrelimCompetition.FollowerPrelimCompetition != null)
                {
                    await SaveDataAsync(PgPrelimCompetitionsTableSchema.TableName, followerPrelimCompetitionModel);
                
                    foreach (var followerPromotedCompetitor in followerPromotedCompetitors)
                    {
                        await SaveDataAsync(PgPromotedCompetitorsTableSchema.TableName, followerPromotedCompetitor);
                    }

                    foreach (var judge in pairedPrelimCompetition.FollowerPrelimCompetition.Judges)
                    {
                        await UpsertJudgeAsync(judge);
                    }
                }

                foreach (var prelimScore in prelimScores)
                {
                    await SaveDataAsync(PgPrelimScoresTableSchema.TableName, prelimScore);
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
                    date_time = competition.FinalCompetition.DateTime
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
                            bib_number = couple.LeaderRegistration.BibNumber
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
                            bib_number = couple.FollowerRegistration.BibNumber
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

                    var followerRecord = finalCompetitorRecords.Where(rec => rec.competitor_registration_id == couple.Follower.CompetitorId).FirstOrDefault();
                    if (followerRecord == null)
                    {
                        var registrationId = 
                            uploadedCompetitorRegistrations.FirstOrDefault(reg => reg.competitor_profile_id == couple.Follower.CompetitorId)?.id ??
                            finalCompetitorRegistrations.First(reg => reg.competitor_profile_id == couple.Follower.CompetitorId).id;

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
                    await SaveDataAsync(PgCompetitorRegistrationsTableSchema.TableName, registration);
                }

                foreach (var record in finalCompetitorRecords)
                {
                    await SaveDataAsync(PgCompetitorRecordsTableSchema.TableName, record);
                }

                await SaveDataAsync(PgFinalCompetitionsTableSchema.TableName, finalCompetitionModel);

                foreach (var placementModel in placements)
                {
                    await SaveDataAsync(PgPlacementsTableSchema.TableName, placementModel);
                }

                foreach (var judge in competition.FinalCompetition.Judges)
                {
                    await UpsertJudgeAsync(judge);
                }

                foreach (var finalScore in finalScores)
                {
                    await SaveDataAsync(PgFinalScoresTableSchema.TableName, finalScore);
                }
            }

            await transaction.CommitAsync();
        }
        public async Task<ICompetition?> GetCompetitionAsync(Guid id)
        {
            ICompetition competition = null;

            string query =
                "SELECT dance_conventions.id, dance_conventions.name, dance_conventions.date, competitions.division"
                + " FROM " + PgCompetitionsTableSchema.TableName
                + " INNER JOIN " + PgDanceConventionsTableSchema.TableName + " ON dance_conventions.id = competitions.dance_convention_id"
                + " WHERE competitions.id = \'" + id + "\';";

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid danceConventionId = reader.GetGuid(0);
                    string danceConventionName = reader.GetString(1);
                    DateTime date = reader.GetDateTime(2);
                    string divisionString = reader.GetString(3);
    
                    Division division = Util.StringToDivision(divisionString);

                    competition = new Competition(danceConventionId, danceConventionName, date, division, id);
                }
            }

            List<IPrelimCompetition> prelimCompetitions = new();

            string prelimCompetitionQuery =
                "SELECT id, date_time, role, round, alternate1_id, alternate2_id"
                + " FROM " + PgPrelimCompetitionsTableSchema.TableName
                + " WHERE prelim_competitions.competition_id = \'" + id + "\';";

            await using (var cmd = _dataSource.CreateCommand(prelimCompetitionQuery))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid prelimCompetitionId = reader.GetGuid(0);
                    DateTime dateTime = reader.GetDateTime(1);
                    string prelimCompetitionRoleString = reader.GetString(2);
                    string prelimCompetitionRoundString = reader.GetString(3);
                    Guid? alternate1Id = null;
                    Guid? alternate2Id = null;

                    try
                    {
                        alternate1Id = reader.GetGuid(4);
                    } catch (InvalidCastException)
                    {
                        alternate1Id = null;
                    }

                    try
                    {
                        alternate2Id = reader.GetGuid(5);
                    }
                    catch (InvalidCastException)
                    {
                        alternate2Id = null;
                    }

                    Role role = Util.StringToRole(prelimCompetitionRoleString);
                    Round round = Util.GetRoundFromString(prelimCompetitionRoundString);

                    prelimCompetitions.Add(new PrelimCompetition(
                        dateTime: dateTime,
                        division: competition.Division,
                        round: round,
                        role: role,
                        prelimScores: null,
                        promotedCompetitors: null,
                        alternate1: App.CompetitorsDb.FirstOrDefault(c => c.CompetitorId == alternate1Id),
                        alternate2: App.CompetitorsDb.FirstOrDefault(c => c.CompetitorId == alternate2Id),
                        id: prelimCompetitionId));
                }
            }

            foreach (var prelimCompetition in prelimCompetitions)
            {
                List<IPrelimScore> prelimScores = new();

                string prelimScoresQuery =
                    "SELECT id, judge_id, competitor_id, callback_score"
                    + " FROM prelim_scores"
                    + " WHERE prelim_competition_id = \'" + prelimCompetition.Id + "\';";

                await using (var cmd = _dataSource.CreateCommand(prelimScoresQuery))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid prelimScoreId = reader.GetGuid(0);
                        Guid competitorId = reader.GetGuid(2);
                        string callbackScore = reader.GetString(3);

                        Guid? judgeId = null;
                        try
                        {
                            judgeId = reader.GetGuid(1);
                        }
                        catch (InvalidCastException)
                        {
                            judgeId = null;
                        }

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
                    + " WHERE prelim_competition_id = \'" + prelimCompetition.Id + "\';";

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
                "SELECT id, date_time"
                + " FROM " + PgFinalCompetitionsTableSchema.TableName
                + " WHERE competition_id = \'" + id + "\';";

            await using (var cmd = _dataSource.CreateCommand(finalCompetitionQuery))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Guid finalCompetitionId = reader.GetGuid(0);
                    DateTime dateTime = reader.GetDateTime(1);

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
                    "SELECT leader_id, follower_id, placement"
                    + " FROM placements"
                    + " WHERE final_competition_id = \'" + finalCompetition.Id + "\';";

                await using (var cmd = _dataSource.CreateCommand(placementsQuery))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int placement = reader.GetInt32(2);

                        Guid? leaderId = null;
                        Guid? followerId = null;
                        try
                        {
                            leaderId = reader.GetGuid(0);
                        }
                        catch (InvalidCastException)
                        {
                            leaderId = null;
                        }

                        try
                        {
                            followerId = reader.GetGuid(1);
                        }
                        catch (InvalidCastException)
                        {
                            followerId = null;
                        }

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
                    + " WHERE final_competition_id = \'" + finalCompetition.Id + "\';";

                await using (var cmd = _dataSource.CreateCommand(finalScoresQuery))
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Guid finalScoreId = reader.GetGuid(0);
                        int score = reader.GetInt32(4);

                        Guid? judgeId = null;
                        Guid? leaderId = null;
                        Guid? followerId = null;

                        try
                        {
                            judgeId = reader.GetGuid(1);
                        }
                        catch (InvalidCastException)
                        {
                            judgeId = null;
                        }

                        try
                        {
                            leaderId = reader.GetGuid(2);
                        }
                        catch (InvalidCastException)
                        {
                            leaderId = null;
                        }

                        try
                        {
                            followerId = reader.GetGuid(3);
                        }
                        catch (InvalidCastException)
                        {
                            followerId = null;
                        }

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
                + " FROM " + PgCompetitionsTableSchema.TableName;

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
            string query = "DELETE FROM " + PgCompetitionsTableSchema.TableName + " WHERE id = \'" + id + "\';";
            await using (var cmd = _dataSource.CreateCommand(query))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            await DeleteHangingCompetitorRegistrations();
        }
        public async Task DeleteAllCompetitionsAsync()
        {
            await DeleteAllDataAsync(PgCompetitionsTableSchema.TableName);
            await DeleteHangingCompetitorRegistrations();
        }

        public void Dispose()
        {
            _dataSource?.Dispose();
        }

        private async Task DeleteHangingCompetitorRegistrations()
        {
            string query = "DELETE FROM " + PgCompetitorRegistrationsTableSchema.TableName
                + " WHERE NOT EXISTS ("
                + "SELECT FROM " + PgCompetitorRecordsTableSchema.TableName
                + " WHERE competitor_records.competitor_registration_id = competitor_registrations.id"
                + ");";

            await using (var cmd = _dataSource.CreateCommand(query))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}