using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseHandler _databaseHandler;
        private const int STARTING_COINS = 20;
        private const int STARTING_ELO = 100;

        public UserRepository()
        {
            _databaseHandler = new DatabaseHandler();
        }

        // Basic CRUD Operations

        /// <summary>
        /// Adds a new user to the database
        /// </summary>
        public void AddUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO users (username, password, coins, elo, wins, losses) 
                    VALUES (@username, @password, @coins, @elo, @wins, @losses)
                    RETURNING id";

                SetUserParameters(command, user);

                // Get the generated ID and set it in the user object
                int id = Convert.ToInt32(command.ExecuteScalar());
                user.SetId(id);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to add user {user.Username}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        public User GetUserById(int userId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM users WHERE id = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            try
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return CreateUserFromDatabaseRow(reader);
                }
                return null!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user with ID {userId}: {ex.Message}");
            }
        }

        public User GetUserByToken(string authToken)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                return null!;
            }

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
                if (reader.Read())
                {
                    var user = CreateUserFromDatabaseRow(reader);
                    // Create and assign token from database data
                    if (!reader.IsDBNull(reader.GetOrdinal("token")))
                    {
                        var tokenValue = reader.GetString(reader.GetOrdinal("token"));
                        var tokenExpiry = reader.GetDateTime(reader.GetOrdinal("token_expiry"));
                        user.AssignToken(new Token(tokenValue, tokenExpiry));
                    }
                    return user;
                }
                return null!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user by token: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a user by their username
        /// </summary>
        public User GetUserByUsername(string username)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM users WHERE username = @username";
            command.Parameters.AddWithValue("@username", username);

            try
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return CreateUserFromDatabaseRow(reader);
                }
                return null!;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user {username}: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a user's profile information
        /// </summary>
        public bool UpdateUserProfile(int userId, UserProfile profile)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE users 
                    SET name = @name, bio = @bio, image = @image
                    WHERE id = @userId";

                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@name", profile.Name);
                command.Parameters.AddWithValue("@bio", profile.Bio);
                command.Parameters.AddWithValue("@image", profile.Image);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to update profile for user {userId}: {ex.Message}");
            }
        }

        // User Stats and Currency Operations

        /// <summary>
        /// Updates a user's stats after a battle
        /// </summary>
        public bool UpdateUserStats(int userId, bool won)
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
                    WHERE id = @userId";

                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@won", won);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to update stats for user {userId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a user's coin balance
        /// </summary>
        public bool UpdateUserCoins(int userId, int amount)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE users 
                    SET coins = GREATEST(0, coins + @amount)
                    WHERE id = @userId";

                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@amount", amount);

                int rowsAffected = command.ExecuteNonQuery();
                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Failed to update coins for user {userId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a user's current coin balance
        /// </summary>
        public int GetUserCoins(int userId)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT coins FROM users WHERE id = @userId";
            command.Parameters.AddWithValue("@userId", userId);

            try
            {
                var result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get coins for user {userId}: {ex.Message}");
            }
        }

        // User Lists and Rankings Operations

        /// <summary>
        /// Gets a list of all users in the system
        /// </summary>
        public List<User> GetAllUsers()
        {
            var userList = new List<User>();

            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM users ORDER BY elo DESC";

            try
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User user = CreateUserFromDatabaseRow(reader);
                    userList.Add(user);
                }
                return userList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get all users: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the scoreboard (users ordered by ELO)
        /// </summary>
        public List<User> GetScoreboard()
        {
            var scoreboard = new List<User>();

            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, username, elo, wins, losses, coins 
                FROM users 
                ORDER BY elo DESC, wins DESC";

            try
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    User user = CreateUserFromDatabaseRow(reader);
                    scoreboard.Add(user);
                }
                return scoreboard;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get scoreboard: {ex.Message}");
            }
        }

        // Validation Operations

        /// <summary>
        /// Checks if a username already exists
        /// </summary>
        public bool UserExists(string username)
        {
            using var connection = _databaseHandler.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username";
            command.Parameters.AddWithValue("@username", username);

            try
            {
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to check if user exists: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a user has enough coins for a purchase
        /// </summary>
        public bool HasEnoughCoins(int userId, int requiredAmount)
        {
            int currentCoins = GetUserCoins(userId);
            return currentCoins >= requiredAmount;
        }

        // Helper Methods

        /// <summary>
        /// Sets the parameters for a user command
        /// </summary>
        private void SetUserParameters(NpgsqlCommand command, User user)
        {
            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@coins", STARTING_COINS);
            command.Parameters.AddWithValue("@elo", STARTING_ELO);
            command.Parameters.AddWithValue("@wins", 0);
            command.Parameters.AddWithValue("@losses", 0);
        }

        /// <summary>
        /// Creates a user object from database row data
        /// </summary>
        private User CreateUserFromDatabaseRow(NpgsqlDataReader reader)
        {
            string username = reader.GetString(reader.GetOrdinal("username"));
            string password = reader.GetString(reader.GetOrdinal("password"));

            var user = new User(username, password);

            user.SetId(reader.GetInt32(reader.GetOrdinal("id")));
            user.InitializeFromDatabase(
                coins: reader.GetInt32(reader.GetOrdinal("coins")),
                elo: reader.GetInt32(reader.GetOrdinal("elo")),
                wins: reader.GetInt32(reader.GetOrdinal("wins")),
                losses: reader.GetInt32(reader.GetOrdinal("losses"))
            );

            // Handle token if present
            if (!reader.IsDBNull(reader.GetOrdinal("token")))
            {
                var tokenValue = reader.GetString(reader.GetOrdinal("token"));
                var tokenExpiry = reader.GetDateTime(reader.GetOrdinal("token_expiry"));
                user.AssignToken(new Token(tokenValue, tokenExpiry));
            }

            return user;
        }

        // Add a method to update token
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
                throw new Exception($"Failed to update token for user {userId}: {ex.Message}");
            }
        }
    }
}
