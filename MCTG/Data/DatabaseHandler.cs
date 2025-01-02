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

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public bool InitializeDatabase()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();

                // 1. Users table: Identity -> Authentication -> Game Stats -> Profile
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
                            bio TEXT,
                            image VARCHAR(255)
                        );";
                    cmd.ExecuteNonQuery();
                }

                // 2. Cards table: Identity -> Card Properties -> Ownership
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
                            
                            -- Ownership & State
                            user_id INT REFERENCES users(id) ON DELETE SET NULL,
                            in_deck BOOLEAN DEFAULT false,
                            
                            -- Constraints
                            CONSTRAINT card_type_check CHECK (card_type IN ('Monster', 'Spell'))
                        );";
                    cmd.ExecuteNonQuery();
                }

                // 3. Trades table: Identity -> Trade Details -> Requirements -> Status
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS trades (
                            -- Identity
                            id SERIAL PRIMARY KEY,
                            
                            -- Trade Details
                            card_id INT REFERENCES cards(id),
                            user_id INT REFERENCES users(id),
                            
                            -- Trade Requirements
                            required_type VARCHAR(20),
                            required_element_type VARCHAR(20),
                            required_monster_type VARCHAR(20),
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
