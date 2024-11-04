using Npgsql;
using System.Data;

namespace MCTG.Data
{
    internal class DatabaseHandler
    {
        private readonly string _connectionString;

        public DatabaseHandler()
        {
            _connectionString = "Host=localhost;Port=5432;Username=mtcg_user;Password=mtcg_password;Database=mtcg_db";
        }

        public IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public void EnsureTableExists()
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                // Create Users table
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS users (
                            id SERIAL PRIMARY KEY,
                            username VARCHAR(50) UNIQUE NOT NULL,
                            password VARCHAR(255) NOT NULL,
                            coins INT DEFAULT 20,
                            elo INT DEFAULT 100,
                            wins INT DEFAULT 0,
                            losses INT DEFAULT 0
                        );";
                    cmd.ExecuteNonQuery();
                }

                // Create Cards table
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS cards (
                            id SERIAL PRIMARY KEY,
                            name VARCHAR(50) NOT NULL,
                            damage INT NOT NULL,
                            element_type VARCHAR(20) NOT NULL,
                            user_id INT REFERENCES users(id) ON DELETE SET NULL
                        );";
                    cmd.ExecuteNonQuery();
                }

                // Create Trades table
                //using (var cmd = conn.CreateCommand())
                //{
                //    cmd.CommandText = @"
                //        CREATE TABLE IF NOT EXISTS trades (
                //            id SERIAL PRIMARY KEY,
                //            card_id INT NOT NULL REFERENCES cards(id) ON DELETE CASCADE,
                //            requested_type VARCHAR(20) NOT NULL,
                //            min_damage INT DEFAULT 0,
                //            active BOOLEAN DEFAULT TRUE
                //        );";
                //    cmd.ExecuteNonQuery();
                //}
            }
        }
    }
}
