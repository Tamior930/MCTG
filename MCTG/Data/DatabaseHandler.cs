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

                // Create Users table
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS users (
                            id SERIAL PRIMARY KEY,
                            username VARCHAR(50) UNIQUE NOT NULL,
                            password VARCHAR(50) NOT NULL,
                            coins INT DEFAULT 20,
                            elo INT DEFAULT 100,
                            wins INT DEFAULT 0,
                            losses INT DEFAULT 0,
                            name VARCHAR(255),
                            bio TEXT,
                            image VARCHAR(255),
                            token VARCHAR(255),
                            token_expiry TIMESTAMP
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
                            card_type VARCHAR(10) NOT NULL,
                            user_id INT REFERENCES users(id) ON DELETE SET NULL,
                            in_deck BOOLEAN DEFAULT false,
                            CONSTRAINT card_type_check CHECK (card_type IN ('Monster', 'Spell'))
                        );";
                    cmd.ExecuteNonQuery();
                }

                // Create Trades table
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS trades (
                            id SERIAL PRIMARY KEY,
                            card_id INT REFERENCES cards(id),
                            user_id INT REFERENCES users(id),
                            required_type VARCHAR(20),
                            required_element_type VARCHAR(20),
                            required_monster_type VARCHAR(20),
                            minimum_damage INT,
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
