using MongoDB.Driver;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ImpartialUI.Services.DatabaseProvider.Helpers
{
    internal class PgHelper : IDisposable
    {
        private string _connectionString;
        private NpgsqlDataSource _dataSource;

        public PgHelper(string connectionString)
        {
            _connectionString = connectionString;
            Load();
        }

        private void Load()
        {
            if (_dataSource == null)
            {
                _dataSource = NpgsqlDataSource.Create(_connectionString);
            }
        }

        public void Dispose()
        {
            _dataSource?.Dispose();
        }

        public async Task<IEnumerable<T>> LoadDataWithQuery<T, U>(string query, U parameters)
        {
            var data = new List<T>();

            await using (var cmd = _dataSource.CreateCommand(query))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                PropertyInfo[] properties = typeof(U).GetProperties();
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

        public async Task<IEnumerable<T>> LoadDataAsync<T, U>(string table, U parameters)
        {
            PropertyInfo[] properties = typeof(U).GetProperties();

            var data = new List<T>();

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

            string command = "SELECT " + columnNames + " FROM " + table;
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

        public async Task SaveDataAsync<U>(string table, U parameters)
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

            await using (var cmd = _dataSource.CreateCommand(command))
            {
                foreach (PropertyInfo property in properties)
                {
                    cmd.Parameters.AddWithValue(property.Name, property.GetValue(parameters));
                }

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
