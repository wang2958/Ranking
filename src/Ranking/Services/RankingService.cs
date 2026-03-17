using Ranking.Api.Models;
using Ranking.Api.Models.Response;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Ranking.Services
{
    /// <summary>
    ///  ConcurrentDictionary<CustomerNode> + Snapshot
    /// </summary>
    public class RankingService : IRankingService
    {
        private volatile bool _isNeedRefreshLeaderboard;
        private volatile List<CustomerNode> _leaderboardSnapshot = new List<CustomerNode>();

        private readonly ConcurrentDictionary<ulong, CustomerNode> _customers = new ConcurrentDictionary<ulong, CustomerNode>();

        public RankingService()
        {
            _ = Task.Factory.StartNew(() => RefreshLeaderboard(), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// O(N logN)
        /// </summary>
        /// <returns></returns>
        async Task RefreshLeaderboard()
        {
            while (true)
            {
                if (_isNeedRefreshLeaderboard)
                {
                    // _lock.EnterReadLock();
                    try
                    {
                        var st = Stopwatch.StartNew();

                        // var newLeaderboardList = _leaderboardSortedSet.ToList();
                        var newLeaderboardList = _customers.Values.Where(v => v.Score > 0).OrderByDescending(v => v.Score).ThenBy(v => v.CustomerId).ToList();
                        _leaderboardSnapshot = newLeaderboardList;

                        var initRank = 1;
                        foreach (var leaderboard in newLeaderboardList)
                        {
                            leaderboard.Rank = initRank++;
                        }

                        _isNeedRefreshLeaderboard = false;

                        st.Stop();
                        Console.WriteLine($"{DateTime.Now}: Refresh Leaderboard {st.ElapsedMilliseconds}ms.");
                    }
                    finally
                    {
                        // _lock.ExitReadLock();
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        /// <summary>
        /// O(1)
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public decimal UpdateScore(ulong customerId, decimal score)
        {
            lock (_customers)
            {
                var customer = _customers.GetOrAdd(customerId, id => new CustomerNode(id, 0));

                customer.Score += score;

                if (customer.Score > 0)
                {
                    _isNeedRefreshLeaderboard = true;
                }
                return customer.Score;
            }
        }

        /// <summary>
        ///  O(K)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public List<GetLeaderboardResponse> GetLeaderboard(int start, int end)
        {
            var result = new List<GetLeaderboardResponse>(end - start + 1);

            var rank = (ulong)start;

            for (int i = start - 1; i < end && i < _leaderboardSnapshot.Count; i++)
            {
                var node = _leaderboardSnapshot[i];

                result.Add(new GetLeaderboardResponse
                {
                    CustomerId = node.CustomerId,
                    Score = node.Score,
                    Rank = rank++
                });
            }
            return result;
        }

        /// <summary>
        /// O(K)
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        public List<GetLeaderboardResponse> GetCustomerLeaderboard(ulong customerId, int high, int low)
        {
            if (!_customers.TryGetValue(customerId, out var customer))
                return new List<GetLeaderboardResponse>();

            int index = customer.Rank - 1;
            if (index < 0) return new List<GetLeaderboardResponse>();

            int start = Math.Max(0, index - high);
            int end = Math.Min(_leaderboardSnapshot.Count - 1, index + low);

            var result = new List<GetLeaderboardResponse>();
            for (int i = start; i <= end; i++)
            {
                result.Add(new GetLeaderboardResponse
                {
                    CustomerId = _leaderboardSnapshot[i].CustomerId,
                    Score = _leaderboardSnapshot[i].Score,
                    Rank = (ulong)(i + 1)
                });
            }

            return result;
        }
    }
}