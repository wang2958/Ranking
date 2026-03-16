using Ranking.Api.Models;
using Ranking.Api.Models.Response;

namespace Ranking.Services
{
    public interface IRankingService
    {

        /// <summary>
        /// O(N)
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public List<GetLeaderboardResponse> GetCustomerLeaderboard(ulong customerId, int high, int low);

        /// <summary>
        ///  O(start + K)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<GetLeaderboardResponse> GetLeaderboard(int start, int end);

        /// <summary>
        /// O(logN)
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public decimal UpdateScore(ulong customerId, decimal score);
    }
}