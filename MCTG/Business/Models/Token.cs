namespace MCTG.Business.Models
{
    public class Token
    {
        public string Value { get; private set; }
        public DateTime ExpiryTime { get; private set; }

        public Token(string value, DateTime expiryTime)
        {
            Value = value;
            ExpiryTime = expiryTime;
        }

        public bool IsValid()
        {
            return DateTime.Now < ExpiryTime;
        }
    }
}
