namespace Ranking.Api.Config
{
    public static class RedisConstant
    {
        public static string GetLeaderboardKey(int start, int end) 
            => $"paas:range:{start}_{end}";

        public static string GetCustomerLeaderboardKey(ulong customerId, int start, int end)
            => $"paas:user_range:{customerId}:{start}_{end}";
    }
}
