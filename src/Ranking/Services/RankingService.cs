using Ranking.Api.Models;
using Ranking.Api.Models.Response;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Ranking.Services
{
    /// <summary>
    /// Dictionary<long, CustomerNode> + SortedSet<CustomerNode> 
    /// </summary>
    public class RankingService : IRankingService
    {
        // private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ConcurrentDictionary<ulong, CustomerNode> _customers = new ConcurrentDictionary<ulong, CustomerNode>();
        // private readonly SortedSet<CustomerNode> _leaderboardSortedSet = new SortedSet<CustomerNode>(new LeaderboardComparer());

        public RankingService()
        {
            _ = Task.Factory.StartNew(
                    () => RefreshLeaderboard(),
                    TaskCreationOptions.LongRunning
                );
        }

        private List<CustomerNode> _leaderboardSnapshot = new List<CustomerNode>();
        private bool _isNeedRefreshLeaderboard;
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
                            leaderboard.Rank = initRank;
                            initRank++;
                        }

                        _isNeedRefreshLeaderboard = false;

                        st.Stop();
                        Console.WriteLine($"{DateTime.Now}: Refresh Leaderboard {st.ElapsedMilliseconds}ms. ");
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
        /// O(logN)
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public decimal UpdateScore(ulong customerId, decimal score)
        {
            // _lock.EnterWriteLock();
            try
            {
                if (!_customers.TryGetValue(customerId, out var customer))
                {
                    customer = new CustomerNode(customerId, score);

                    _customers[customerId] = customer;

                    if (customer.Score > 0)
                    {
                        // _leaderboardSortedSet.Add(customer);
                        _isNeedRefreshLeaderboard = true;
                    }

                    return customer.Score;
                }

                if (customer.Score > 0)
                {
                    // _leaderboardSortedSet.Remove(customer);
                    _isNeedRefreshLeaderboard = true;
                }
                customer.Score += score;
                if (customer.Score > 0)
                {
                    // _leaderboardSortedSet.Add(customer);
                    _isNeedRefreshLeaderboard = true;
                }

                return customer.Score;
            }
            finally
            {
                // _lock.ExitWriteLock();
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
        /// O(low+high)
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
                result.Add(new GetLeaderboardResponse { CustomerId = _leaderboardSnapshot[i].CustomerId, Score = _leaderboardSnapshot[i].Score, Rank = (ulong)(i + 1) });
            }

            return result;
        }
    }
}