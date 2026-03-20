using Microsoft.Extensions.Caching.Memory;
using Ranking.Api.Collections;
using Ranking.Api.Config;
using Ranking.Api.Models.Response;
using Ranking.Services;
using System.Collections.Concurrent;

namespace Ranking.Api.Services
{
    public class RankingSkipListService : IRankingService
    {
        private readonly SkipList _skipList = new SkipList();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<ulong, decimal> _customers = new Dictionary<ulong, decimal>(capacity: 50 * 10000);

        IMemoryCache _memoryCache;
        private readonly ConcurrentBag<string> allCacheKeys = new ConcurrentBag<string>();

        public RankingSkipListService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public decimal UpdateScore(ulong customerId, decimal score)
        {
            try
            {
                _lock.EnterWriteLock();

                if (_customers.TryGetValue(customerId, out var customerScore))
                {
                    if (customerScore > 0) _skipList.Delete(customerId, customerScore);
                    customerScore += score;
                    _customers[customerId] = customerScore;
                }
                else
                {
                    customerScore = score;
                    _customers.TryAdd(customerId, customerScore);
                }

                if (customerScore > 0) _skipList.Insert(customerId, customerScore);

                // RemoveCache();

                return customerScore;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        void RemoveCache()
        {
            foreach (var cacheKey in allCacheKeys)
            {
                _memoryCache.Remove(cacheKey);
            }
        }

        List<GetLeaderboardResponse> EmptyListResponse = new List<GetLeaderboardResponse>();

        public List<GetLeaderboardResponse> GetLeaderboard(int start, int end)
        {
            start = Math.Max(1, start);

            return GetLeaderboardWithSkipList(start, end);

            //var key = RedisConstant.GetLeaderboardKey(start, end);
            //return _memoryCache.GetOrCreate(key, (entry) =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    allCacheKeys.Add(key);

            //    return GetLeaderboardWithSkipList(start, end);
            //}) ?? EmptyListResponse;
        }

        private List<GetLeaderboardResponse> GetLeaderboardWithSkipList(int start, int end)
        {
            try
            {
                _lock.EnterReadLock();

                if (start > _skipList.Length)
                {
                    return EmptyListResponse;
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
            return GetCustomerLeaderboardWithSkipList(customerId, high, low);

            //var key = RedisConstant.GetCustomerLeaderboardKey(customerId, high, low);
            //return _memoryCache.GetOrCreate(key, (entry) =>
            //{
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            //    allCacheKeys.Add(key);

            //    return GetCustomerLeaderboardWithSkipList(customerId, high, low);
            //}) ?? EmptyListResponse; 
        }

        private List<GetLeaderboardResponse> GetCustomerLeaderboardWithSkipList(ulong customerId, int high, int low)
        {
            try
            {
                _lock.EnterReadLock();

                if (!_customers.TryGetValue(customerId, out var customerScore))
                {
                    return EmptyListResponse;
                }

                if (customerScore <= 0)
                {
                    return EmptyListResponse;
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
