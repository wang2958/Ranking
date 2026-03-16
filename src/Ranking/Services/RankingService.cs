using Ranking.Api.Models;
using Ranking.Api.Models.Response;

namespace Ranking.Services
{
    public class RankingService : IRankingService
    { 
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<ulong, CustomerNode> _customers = new Dictionary<ulong, CustomerNode>();
        private readonly SortedSet<CustomerNode> _leaderboard = new SortedSet<CustomerNode>(new LeaderboardComparer());
         
        /// <summary>
        /// O(logN)
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public decimal UpdateScore(ulong customerId, decimal score)
        {
            _lock.EnterWriteLock();
            try
            { 
                if (!_customers.TryGetValue(customerId, out var customer))
                {
                    customer = new CustomerNode(customerId, score);

                    _customers[customerId] = customer;

                    if (customer.Score > 0) _leaderboard.Add(customer);

                    return customer.Score;
                }

                if (customer.Score > 0) _leaderboard.Remove(customer);
                customer.Score += score;
                if (customer.Score > 0) _leaderboard.Add(customer);

                return customer.Score;
            }
            finally
            {
                _lock.ExitWriteLock();
            } 
        }

        /// <summary>
        ///  O(start + K)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<GetLeaderboardResponse> GetLeaderboard(int start, int end)
        {
            _lock.EnterReadLock();

            try
            {
                ulong index = (ulong)start;
                return _leaderboard.Skip(start - 1).Take(end - start + 1).Select(c => new GetLeaderboardResponse { CustomerId = c.CustomerId, Score = c.Score, Rank = index++ }).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// O(N)
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public List<GetLeaderboardResponse> GetCustomerLeaderboard(ulong customerId, int high, int low)
        {
            _lock.EnterReadLock();

            try
            {
                if (!_customers.TryGetValue(customerId, out var customer))
                    return new List<GetLeaderboardResponse>();

                var list = _leaderboard.ToList();
                int index = list.IndexOf(customer);

                if (index < 0) return new List<GetLeaderboardResponse>();

                int start = Math.Max(0, index - high);
                int end = Math.Min(list.Count - 1, index + low);

                var result = new List<GetLeaderboardResponse>();
                for (int i = start; i <= end; i++)
                {
                    result.Add(new GetLeaderboardResponse { CustomerId = list[i].CustomerId, Score = list[i].Score, Rank = (ulong)(i + 1) });
                }

                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}