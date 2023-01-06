using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Impartial
{
    public class SqlHelper
    {
        private string _connectionString;

        public SqlHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<T> LoadData<T, U>(string storedProcedure, U parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                return connection.Query<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public void SaveData<T>(string storedProcedure, T parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                connection.Execute(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IEnumerable<T>> LoadDataAsync<T, U>(string storedProcedure, U parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task SaveDataAsync<T>(string storedProcedure, T parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
