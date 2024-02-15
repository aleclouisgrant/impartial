using System;
using Npgsql;

namespace ImpartialUI
{
    internal static class Migrator
    {
        public static async void Migrate()
        {
            string pgConnectionString =
                String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    "localhost",
                    "postgres",
                    "WCS-SS-DB",
                    "5432",
                    "*Firenice18");

            await using var pgConnection = new NpgsqlConnection(pgConnectionString);
            await pgConnection.OpenAsync();

            var sqlDb = App.DatabaseProvider;
            var sqlCompetitors = await sqlDb.GetAllCompetitorsAsync();

            string pgUsersInsertCommand = "INSERT INTO users (first_name, last_name, id) VALUES (@FirstName, @LastName, @Id)";
            string pgCompetitorInsertCommand = "INSERT INTO competitor_profiles (user_id, wsdc_id) VALUES (@UserId, @WsdcId)";

            foreach (var competitor in sqlCompetitors)
            {
                await using (var cmd = new NpgsqlCommand(pgUsersInsertCommand, pgConnection))
                {
                    cmd.Parameters.AddWithValue("FirstName", competitor.FirstName);
                    cmd.Parameters.AddWithValue("LastName", competitor.LastName);
                    cmd.Parameters.AddWithValue("Id", competitor.Id);
                    await cmd.ExecuteNonQueryAsync();
                }

                await using (var cmd = new NpgsqlCommand(pgCompetitorInsertCommand, pgConnection))
                {
                    cmd.Parameters.AddWithValue("UserId", competitor.Id);
                    cmd.Parameters.AddWithValue("WsdcId", competitor.WsdcId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
