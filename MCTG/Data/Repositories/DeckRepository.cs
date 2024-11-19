using MCTG.BusinessLayer.Models;
using MCTG.Data.Interfaces;
using Npgsql;

namespace MCTG.Data.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly DatabaseHandler _dbHandler;

        public DeckRepository()
        {
            _dbHandler = new DatabaseHandler();
        }

        public void SaveDeck(int userId, List<Card> cards)
        {
            throw new NotImplementedException();
        }

        public List<Card> GetDeckByUserId(int userId)
        {
            throw new NotImplementedException();
        }

        public void AddCardToDeck(int userId, Card card)
        {
            throw new NotImplementedException();
        }

        public void RemoveCardFromDeck(int userId, Card card)
        {
            throw new NotImplementedException();
        }

        public int GetDeckCount(int userId)
        {
            using var conn = _dbHandler.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM cards WHERE user_id = @userId AND in_deck = true";
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
} 