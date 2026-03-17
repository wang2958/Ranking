# Ranking

一个 高性能排行榜服务实现（.NET）

该项目通过 ConcurrentDictionary + Snapshot（快照）架构 实现：

- 写入 O(1)
- 查询 O(K)
- 排行榜更新 O(N logN)

通过后台线程异步刷新排行榜，将高成本排序操作从用户请求路径中剥离，从而获得极高的查询吞吐量。

# 架构设计

整体架构如下：

    +--------------------+
                |   Update Score     |
                |   O(1)             |
                +---------+----------+
                          |
                          v
               ConcurrentDictionary
                     (全量数据)
                          |
                          | 触发刷新
                          v
                 Refresh Thread
               (后台排行榜刷新)
                    O(N logN)
                          |
                          v
                  Snapshot List
                 (排行榜快照)
                          |
                          |
            +-------------+-------------+
            |                           |
            v                           v
     GetLeaderboard()            GetCustomerLeaderboard()
           O(K)                         O(K)

核心思想：

- 写入更新只修改 ConcurrentDictionary
- 排行榜查询只访问 Snapshot 快照
- 后台线程负责周期性排序生成快照

# 核心复杂度

| 操作                   | 时间复杂度           | 说明               |
| ---------------------- | -------------------- | ------------------ |
| UpdateScore            | **O(1)**       | 更新用户分数       |
| GetLeaderboard         | **O(K)**       | 获取区间排行榜     |
| GetCustomerLeaderboard | **O(K)**       | 获取用户附近排行榜 |
| RefreshLeaderboard     | **O(N log N)** | 后台刷新排行榜     |

# JMeter 压测

压测环境：

```
CPU: 8C16T
Memory: 64GB
.NET: .NET 10
OS: Linux

JMeter: 
Threads: 200
LoopCount: 5000
初始客户数据50w, 初始排行榜数据50w
```

排行榜刷新延迟 ≈ 500 ms

单接口测试结果
![](img/single_update_customer_score.png)
![](img/single_leader_board.png)
![](img/single_leaderboard_customer.png)

聚合测试结果
![](img/agg.png)