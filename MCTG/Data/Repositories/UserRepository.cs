using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseHandler _databaseHandler;

        public UserRepository()
        {
            _databaseHandler = new DatabaseHandler();
        }

        public void AddUser(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO users (username, password, coins, elo, wins, losses, bio, image) 
                    VALUES (@username, @password, @coins, @elo, @wins, @losses, @bio, @image)
                    RETURNING id";

                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@password", user.Password);
                command.Parameters.AddWithValue("@coins", user.Coins);
                command.Parameters.AddWithValue("@elo", user.ELO);
                command.Parameters.AddWithValue("@wins", user.Wins);
                command.Parameters.AddWithValue("@losses", user.Losses);
                command.Parameters.AddWithValue("@bio", user.Profile?.Bio ?? "");
                command.Parameters.AddWithValue("@image", user.Profile?.Image ?? "");

                user.SetId(Convert.ToInt32(command.ExecuteScalar()));
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to add user {user.Username}: {ex.Message}");
            }
        }

        public User GetUserByToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken)) return null!;

            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                SELECT * FROM users 
                WHERE token = @token 
                AND token_expiry > CURRENT_TIMESTAMP";
            command.Parameters.AddWithValue("@token", authToken);

            try
            {
                using var reader = command.ExecuteReader();
                return reader.Read() ? MapUserFromDatabase(reader) : null!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user by token: {ex.Message}");
            }
        }

        public bool UpdateUserProfile(string authToken, UserProfile profile)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE users 
                    SET bio = @bio,
                        image = @image
                    WHERE token = @token
                    AND token_expiry > CURRENT_TIMESTAMP";

                command.Parameters.AddWithValue("@token", authToken);
                command.Parameters.AddWithValue("@bio", profile.Bio ?? "");
                command.Parameters.AddWithValue("@image", profile.Image ?? "");

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to update user profile: {ex.Message}");
            }
        }

        public bool UpdateUserStats(string authToken, bool won)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE users 
                    SET elo = CASE WHEN @won THEN elo + 3 ELSE GREATEST(0, elo - 5) END,
                        wins = CASE WHEN @won THEN wins + 1 ELSE wins END,
                        losses = CASE WHEN @won THEN losses ELSE losses + 1 END
                    WHERE token = @token";

                command.Parameters.AddWithValue("@token", authToken);
                command.Parameters.AddWithValue("@won", won);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to update stats: {ex.Message}");
            }
        }

        public bool UpdateUserCoins(int userId, int amount)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "UPDATE users SET coins = coins + @amount WHERE id = @userId";
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@amount", amount);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        private User MapUserFromDatabase(NpgsqlDataReader reader)
        {
            var user = new User(
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetString(reader.GetOrdinal("password"))
            );

            user.SetId(reader.GetInt32(reader.GetOrdinal("id")));
            user.InitializeFromDatabase(
                reader.GetInt32(reader.GetOrdinal("coins")),
                reader.GetInt32(reader.GetOrdinal("elo")),
                reader.GetInt32(reader.GetOrdinal("wins")),
                reader.GetInt32(reader.GetOrdinal("losses"))
            );

            if (!reader.IsDBNull(reader.GetOrdinal("token")))
            {
                user.AssignToken(new Token(
                    reader.GetString(reader.GetOrdinal("token")),
                    reader.GetDateTime(reader.GetOrdinal("token_expiry"))
                ));
            }

            return user;
        }

        public List<User> GetScoreboard()
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM users ORDER BY elo DESC, wins DESC";

            try
            {
                var scoreboard = new List<User>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    scoreboard.Add(MapUserFromDatabase(reader));
                }
                return scoreboard;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get scoreboard: {ex.Message}");
            }
        }

        public bool UserExists(string username)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username";
            command.Parameters.AddWithValue("@username", username);

            try
            {
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check if user exists: {ex.Message}");
            }
        }

        public bool UpdateUserToken(int userId, Token token)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE users 
                    SET token = @token, token_expiry = @expiry
                    WHERE id = @userId";

                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@token", token.Value);
                command.Parameters.AddWithValue("@expiry", token.ExpiryTime);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to update token: {ex.Message}");
            }
        }
    }
}
