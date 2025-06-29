namespace VotingSystem.API.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public string Option { get; set; } 
        public DateTime Timestamp { get; set; }
    }
}
