namespace Ranking.Api.Models
{
    public class CustomerNode
    {
        public ulong CustomerId { get; set; }
        public decimal Score { get; set; }
        public int Rank { get; set; }

        public CustomerNode() { }

        public CustomerNode(ulong customerId, decimal score)
        {
            this.CustomerId = customerId;
            this.Score = score;
        }
    } 
}