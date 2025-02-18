# Monster Card Trading Game (MCTG)

A server-based card trading game where players can battle with fantasy creatures and spells, trade cards, and compete on the leaderboard.

## Features

- **User Management**
  - Registration and authentication
  - Profile customization
  - Virtual currency system

- **Card System**
  - Unique cards with different elements and types
  - Card package purchases
  - Deck configuration (4 cards per deck)

- **Trading System**
  - Create trading deals
  - Trade cards with other players
  - Card ownership validation

- **Battle System**
  - Turn-based battles
  - Element-based damage calculation
  - ELO rating system
  - Battle statistics tracking

- **Scoreboard**
  - Global player rankings
  - Win/loss statistics

## Technology Stack

- **Backend**: C# (.NET 8.0)
- **Database**: PostgreSQL
- **Testing**: NUnit
- **HTTP Server**: Custom TCP-based implementation
- **Dependencies**: 
  - Npgsql (PostgreSQL driver)
  - Moq (Mocking framework)
  - NUnit (Testing framework)

## Setup

1. **Prerequisites**
   - .NET 8.0 SDK
   - PostgreSQL database server
   - Visual Studio 2022 (recommended) or any .NET-compatible IDE

2. **Database Setup**
   ```sql
   -- Create database and user
   CREATE DATABASE mtcg_db;
   CREATE USER mtcg_user WITH PASSWORD 'mtcg_password';
   GRANT ALL PRIVILEGES ON DATABASE mtcg_db TO mtcg_user;
   ```

3. **Project Setup**
   ```bash
   # Clone the repository
   git clone [repository-url]
   cd MCTG

   # Restore dependencies
   dotnet restore

   # Build the project
   dotnet build
   ```

4. **Running the Server**
   ```bash
   # Start the server (listens on port 10001 by default)
   dotnet run
   ```

5. **Testing**
   ```bash
   # Run the test suite
   dotnet test
   ```

## API Endpoints

### User Management
- `POST /users` - Register new user
- `POST /sessions` - User login
- `GET /users/{username}` - Get user data
- `PUT /users/{username}` - Update user data

### Cards & Deck
- `POST /cards/packages` - Buy card package
- `GET /cards` - Show user's cards
- `GET /deck` - Show user's deck
- `PUT /deck` - Configure deck

### Trading
- `GET /tradings` - Show trading deals
- `POST /tradings` - Create trading deal
- `DELETE /tradings/{tradingId}` - Delete trading deal
- `POST /tradings/{tradingId}` - Execute trade

### Battle & Stats
- `POST /battles` - Enter battle
- `GET /stats` - Show user stats
- `GET /score` - Show scoreboard

## Testing Script

A Windows command script (`script.cmd`) is provided for testing the API endpoints. Run it after starting the server to test various functionalities.

## Project Structure

- `Business/Models/` - Game entities and logic
- `Data/` - Database access and repositories
- `Presentation/` - HTTP server, controllers, and routing
- `Tests/` - Unit tests

## Contributors

[Tamior930]
