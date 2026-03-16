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

    public class LeaderboardComparer : IComparer<CustomerNode>
    {
        public int Compare(CustomerNode x, CustomerNode y)
        {
            if (x == null || y == null) return 0;

            int scoreCompare = y.Score.CompareTo(x.Score);

            if (scoreCompare != 0)
                return scoreCompare;

            return x.CustomerId.CompareTo(y.CustomerId);
        }
    }
}