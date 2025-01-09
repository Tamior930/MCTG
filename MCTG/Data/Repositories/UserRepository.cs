using MCTG.Business.Models;
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

        public User GetUserByUsername(string username)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM users WHERE username = @username";
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapUserFromDatabase(reader) : null!;
        }

        public bool UpdateUserProfile(int userId, UserProfile profile)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = @"
                UPDATE users 
                SET bio = @bio, image = @image 
                WHERE id = @userId
                RETURNING id";

            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@bio", profile.Bio);
            command.Parameters.AddWithValue("@image", profile.Image);

            try
            {
                var result = command.ExecuteScalar();
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                return false;
            }
        }

        public bool UpdateUserStats(string authToken, bool won)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            int eloChange = won ? 3 : -5;

            command.CommandText = @"
                UPDATE users 
                SET elo = elo + @eloChange,
                    wins = CASE WHEN @won THEN wins + 1 ELSE wins END,
                    losses = CASE WHEN @won THEN losses ELSE losses + 1 END
                WHERE token = @token
                RETURNING id";

            command.Parameters.AddWithValue("@eloChange", eloChange);
            command.Parameters.AddWithValue("@won", won);
            command.Parameters.AddWithValue("@token", authToken);

            try
            {
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user stats: {ex.Message}");
                return false;
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

            var profile = new UserProfile(
                reader.IsDBNull(reader.GetOrdinal("bio")) ? "" : reader.GetString(reader.GetOrdinal("bio")),
                reader.IsDBNull(reader.GetOrdinal("image")) ? "" : reader.GetString(reader.GetOrdinal("image"))
            );
            user.Profile = profile;

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

        public User? GetUserById(int userId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM users WHERE id = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapUserFromDatabase(reader);
            }
            return null;
        }
    }
}
