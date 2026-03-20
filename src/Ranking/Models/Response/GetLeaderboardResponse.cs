namespace Ranking.Api.Models.Response
{
    public class GetLeaderboardResponse
    {
        public ulong CustomerId { get; set; }
        public decimal Score { get; set; }
        public int Rank { get; set; }
    }
}