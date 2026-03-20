namespace Ranking.Api.Models.Response
{
    public struct GetLeaderboardResponse
    {
        public ulong CustomerId { get; set; }
        public decimal Score { get; set; }
        public int Rank { get; set; }
    }
}