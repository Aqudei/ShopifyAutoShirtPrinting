using Npgsql;

namespace ShopifyEasyShirtPrinting.Data
{
    public class DbService
    {
        private readonly string _connectionString;
        private readonly NpgsqlConnection _connection;

        public DbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ResetDatabase()
        {
            using (var _connection = new NpgsqlConnection(_connectionString))
            {
                _connection.Open();
                var cmd = _connection.CreateCommand();
                cmd.CommandText = "call public.reset_database()";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
                _connection.Close();
            }
        }
    }
}
