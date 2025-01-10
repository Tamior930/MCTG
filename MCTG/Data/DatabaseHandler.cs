using Npgsql;

namespace MCTG.Data
{
    internal class DatabaseHandler
    {
        private readonly string _connectionString;

        public DatabaseHandler()
        {
            _connectionString = "Host=localhost;Port=5432;Username=mtcg_user;Password=mtcg_password;Database=mtcg_db";
        }

        // Creates and returns a new database connection
        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        // Sets up database schema if not exists
        public bool InitializeDatabase()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();

                // Create users table with identity, auth, stats and profile columns
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS users (
                            -- Identity
                            id SERIAL PRIMARY KEY,
                            username VARCHAR(50) UNIQUE NOT NULL,
                            
                            -- Authentication
                            password VARCHAR(50) NOT NULL,
                            token VARCHAR(255),
                            token_expiry TIMESTAMP,
                            
                            -- Game Stats
                            coins INT DEFAULT 20,
                            elo INT DEFAULT 100,
                            wins INT DEFAULT 0,
                            losses INT DEFAULT 0,
                            
                            -- Profile Data
                            bio text,
                            image text
                        );";
                    cmd.ExecuteNonQuery();
                }

                // Create cards table with properties and ownership tracking
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS cards (
                            -- Identity
                            id SERIAL PRIMARY KEY,
                            
                            -- Card Properties
                            name VARCHAR(50) NOT NULL,
                            damage INT NOT NULL,
                            element_type VARCHAR(20) NOT NULL,
                            card_type VARCHAR(10) NOT NULL,
                            monster_type VARCHAR(20),
                            
                            -- Ownership & State
                            user_id INT REFERENCES users(id) ON DELETE SET NULL,
                            in_deck BOOLEAN DEFAULT false,
                            deck_order INT,
                            
                            -- Constraints
                            CONSTRAINT card_type_check CHECK (card_type IN ('Monster', 'Spell')),
                            CONSTRAINT monster_type_check CHECK (
                                (card_type = 'Monster' AND monster_type IS NOT NULL) OR
                                (card_type = 'Spell' AND monster_type IS NULL)
                            ),
                            CONSTRAINT deck_order_range CHECK (deck_order IS NULL OR deck_order BETWEEN 1 AND 4)
                        );";
                    cmd.ExecuteNonQuery();
                }

                // Create trades table for managing card trading
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS trades (
                            -- Identity
                            id SERIAL PRIMARY KEY,
                            
                            -- Trade Details
                            card_id INT REFERENCES cards(id),
                            user_id INT REFERENCES users(id),
                            required_type VARCHAR(10) CHECK (required_type IN ('spell', 'monster')),
                            minimum_damage INT,
                            
                            -- Status
                            status VARCHAR(20) DEFAULT 'ACTIVE',
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        );";
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Database initialized successfully!");
                return true;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during database initialization: {ex.Message}");
                return false;
            }
        }
    }
}
