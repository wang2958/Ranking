# Ranking

一个高性能排行榜服务实现（.NET）

该项目通过 Skip List（跳表）+ ReaderWriterLockSlim 架构实现：

- 强一致性：更新操作完成后，排名立即生效。
- 高性能查询：利用跳表的索引和跨度（Span）属性，在 $O(\log N)$ 时间内完成排名计算。
- 高吞吐量：通过读写锁（ReaderWriterLockSlim）实现多线程并发读取，优化读多写少的排行榜场景。



# 架构设计

整体架构如下：

      +--------------------------+
      |   API Request (Update)   |
      +-----------+--------------+
                  |
                  v
      ReaderWriterLock (Write)
                  |
      +-----------+--------------+
      |    Dictionary<ID, Score> | 
      |    Skip List (With Span) | 
      +-----------+--------------+
                  |
                  v
      +-----------+--------------+
      |   API Request (Query)    |
      +-----------+--------------+
                  |
                  v
      ReaderWriterLock (Read)
                  |
      Skip List Traversal (O(log N + K))

核心思想：

- 跳表跨度（Span）：每个节点记录了跳跃步长，时间复杂度降至 $O(\log N)$。
- 读写分离锁：允许同时处理多个查询请求，只有在更新分数时才会阻塞读取。
- 使用 Dictionary 存储用户分数的实时快照，避免在跳表中进行全表扫描找用户。

# 核心复杂度

| 操作                   | 时间复杂度           | 说明               |
| ---------------------- | -------------------- | ------------------ |
| UpdateScore            | **O(log N)**       | 更新用户分数       |
| GetLeaderboard         | **O(log N)**       | 获取区间排行榜     |
| GetCustomerLeaderboard | **O(log N + K)**       | 获取用户附近排行榜 |

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

单接口测试结果
![](img/single_update_customer_score.png)
![](img/single_leader_board.png)
![](img/single_leaderboard_customer.png)

聚合测试结果
![](img/agg.png)