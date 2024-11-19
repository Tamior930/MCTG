using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using System.Data;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseHandler _dbHandler;

        public UserRepository()
        {
            _dbHandler = new DatabaseHandler();
        }

        public void AddUser(User user)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO users (username, password, coins, elo, wins, losses) 
                    VALUES (@username, @password, @coins, @elo, @wins, @losses)
                    RETURNING id";

                cmd.Parameters.Add(new NpgsqlParameter("@username", user.Username));
                cmd.Parameters.Add(new NpgsqlParameter("@password", user.Password));
                cmd.Parameters.Add(new NpgsqlParameter("@coins", user.Coins));
                cmd.Parameters.Add(new NpgsqlParameter("@elo", user.ELO));
                cmd.Parameters.Add(new NpgsqlParameter("@wins", user.Wins));
                cmd.Parameters.Add(new NpgsqlParameter("@losses", user.Losses));

                // Get the generated ID and set it in the user object
                int id = Convert.ToInt32(cmd.ExecuteScalar());
                user.SetId(id);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public User GetUserByUsername(string username)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM users WHERE username = @username";
            cmd.Parameters.Add(new NpgsqlParameter("@username", username));

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User(
                    id: reader.GetInt32(reader.GetOrdinal("id")),
                    username: reader.GetString(reader.GetOrdinal("username")),
                    password: reader.GetString(reader.GetOrdinal("password")),
                    coins: reader.GetInt32(reader.GetOrdinal("coins")),
                    elo: reader.GetInt32(reader.GetOrdinal("elo")),
                    wins: reader.GetInt32(reader.GetOrdinal("wins")),
                    losses: reader.GetInt32(reader.GetOrdinal("losses"))
                );
            }
            return null;
        }

        public bool RemoveUserByUsername(string username)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM users WHERE username = @username";
            cmd.Parameters.Add(new NpgsqlParameter("@username", username));

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UserExists(string username)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username";
            cmd.Parameters.Add(new NpgsqlParameter("@username", username));

            return cmd.ExecuteScalar() is long count && count > 0;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM users";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var user = new User(
                    reader.GetString(reader.GetOrdinal("username")),
                    reader.GetString(reader.GetOrdinal("password"))
                );
                // Additional properties will be set through constructor
                users.Add(user);
            }

            return users;
        }
    }
}
