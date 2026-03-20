using Ranking.Api.Collections;
using Ranking.Api.Models.Response;
using Ranking.Services;

namespace Ranking.Api.Services
{
    public class RankingSkipListService : IRankingService
    {
        private readonly SkipList _skipList = new SkipList();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<ulong, decimal> _customers = new Dictionary<ulong, decimal>(capacity: 50 * 10000);

        public decimal UpdateScore(ulong customerId, decimal score)
        {
            try
            {
                _lock.EnterWriteLock();

                if (_customers.TryGetValue(customerId, out var customerScore))
                {
                    if (customerScore > 0) _skipList.Delete(customerId, score);
                    customerScore += score;
                    _customers[customerId] = customerScore;
                }
                else
                {
                    customerScore = score;
                    _customers.TryAdd(customerId, customerScore);
                }

                if (customerScore > 0) _skipList.Insert(customerId, customerScore);

                return customerScore;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public List<GetLeaderboardResponse> GetLeaderboard(int start, int end)
        {
            try
            {
                start = Math.Max(1, start);

                _lock.EnterReadLock();

                if (start > _skipList.Length)
                {
                    return new List<GetLeaderboardResponse>();
                }

                return _skipList.GetRange(start, end).Select(x => new GetLeaderboardResponse
                {
                    CustomerId = x.CustomerId,
                    Score = x.Score,
                    Rank = start++
                }).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<GetLeaderboardResponse> GetCustomerLeaderboard(ulong customerId, int high, int low)
        {
            try
            { 
                _lock.EnterReadLock();

                if (!_customers.TryGetValue(customerId, out var customerScore))
                {
                    return new List<GetLeaderboardResponse>();
                }

                if (customerScore <= 0)
                {
                    return new List<GetLeaderboardResponse>();
                }

                int rank = _skipList.GetRank(customerId, customerScore);
                var start = Math.Max(1, rank - high);
                var end = rank + low;
                return _skipList.GetRange(start, end).Select(x => new GetLeaderboardResponse
                {
                    CustomerId = x.CustomerId,
                    Score = x.Score,
                    Rank = start++
                }).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
