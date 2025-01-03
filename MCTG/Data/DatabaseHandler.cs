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
                        );

                        -- Seed initial cards if table is empty
                        INSERT INTO cards (name, damage, element_type, card_type, user_id, in_deck)
                        SELECT * FROM (VALUES
                            -- Fire Monster Cards
                            ('FireDragon', 65, 'Fire', 'Monster', NULL, false),
                            ('FireGoblin', 30, 'Fire', 'Monster', NULL, false),
                            ('FireTroll', 40, 'Fire', 'Monster', NULL, false),
                            ('FireElf', 25, 'Fire', 'Monster', NULL, false),
                            ('FireDwarf', 35, 'Fire', 'Monster', NULL, false),

                            -- Water Monster Cards
                            ('WaterDragon', 60, 'Water', 'Monster', NULL, false),
                            ('WaterGoblin', 25, 'Water', 'Monster', NULL, false),
                            ('Kraken', 55, 'Water', 'Monster', NULL, false),
                            ('WaterTroll', 35, 'Water', 'Monster', NULL, false),
                            ('WaterElf', 22, 'Water', 'Monster', NULL, false),

                            -- Normal Monster Cards
                            ('Dragon', 55, 'Normal', 'Monster', NULL, false),
                            ('Goblin', 28, 'Normal', 'Monster', NULL, false),
                            ('Troll', 38, 'Normal', 'Monster', NULL, false),
                            ('Knight', 35, 'Normal', 'Monster', NULL, false),
                            ('Ork', 45, 'Normal', 'Monster', NULL, false),
                            ('Wizard', 40, 'Normal', 'Monster', NULL, false),

                            -- Fire Spell Cards
                            ('FireSpell', 45, 'Fire', 'Spell', NULL, false),
                            ('FireBall', 35, 'Fire', 'Spell', NULL, false),
                            ('FireBlast', 25, 'Fire', 'Spell', NULL, false),
                            ('FireStorm', 55, 'Fire', 'Spell', NULL, false),

                            -- Water Spell Cards
                            ('WaterSpell', 40, 'Water', 'Spell', NULL, false),
                            ('WaterBlast', 35, 'Water', 'Spell', NULL, false),
                            ('Tsunami', 50, 'Water', 'Spell', NULL, false),
                            ('WaterBall', 30, 'Water', 'Spell', NULL, false),

                            -- Normal Spell Cards
                            ('RegularSpell', 28, 'Normal', 'Spell', NULL, false),
                            ('NormalBlast', 35, 'Normal', 'Spell', NULL, false),
                            ('EnergyBall', 40, 'Normal', 'Spell', NULL, false),
                            ('MagicMissile', 25, 'Normal', 'Spell', NULL, false)
                        ) AS seed_data(name, damage, element_type, card_type, user_id, in_deck)
                        WHERE NOT EXISTS (SELECT 1 FROM cards LIMIT 1);";

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
