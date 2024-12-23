using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
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

        public bool UpdateUserStats(int userId, bool won)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE users 
                    SET elo = CASE WHEN @won THEN elo + 3 ELSE GREATEST(0, elo - 5) END,
                        wins = CASE WHEN @won THEN wins + 1 ELSE wins END,
                        losses = CASE WHEN @won THEN losses ELSE losses + 1 END
                    WHERE id = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@won", won);

                int rowsAffected = cmd.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public bool UpdateUserCoins(int userId, int amount)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE users 
                    SET coins = GREATEST(0, coins + @amount)
                    WHERE id = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@amount", amount);

                int rowsAffected = cmd.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public List<User> GetScoreboard()
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, username, elo, wins, losses, coins 
                FROM users 
                ORDER BY elo DESC, wins DESC";

            var users = new List<User>();
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                var user = new User(
                    id: reader.GetInt32(reader.GetOrdinal("id")),
                    username: reader.GetString(reader.GetOrdinal("username")),
                    password: string.Empty,  // We don't expose passwords in scoreboard
                    coins: reader.GetInt32(reader.GetOrdinal("coins")),
                    elo: reader.GetInt32(reader.GetOrdinal("elo")),
                    wins: reader.GetInt32(reader.GetOrdinal("wins")),
                    losses: reader.GetInt32(reader.GetOrdinal("losses"))
                );
                users.Add(user);
            }

            return users;
        }

        public bool UpdateUserProfile(int userId, UserProfile profile)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();
            using var transaction = conn.BeginTransaction();

            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE users 
                    SET name = @name, bio = @bio, image = @image
                    WHERE id = @userId";
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@name", profile.Name);
                cmd.Parameters.AddWithValue("@bio", profile.Bio);
                cmd.Parameters.AddWithValue("@image", profile.Image);

                int rowsAffected = cmd.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public int GetUserCoins(int userId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT coins FROM users WHERE id = @userId";
            cmd.Parameters.AddWithValue("@userId", userId);

            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
