# Ranking

A high-performance leaderboard service implemented in .NET.

This project leverages a Skip List with Span combined with a ReaderWriterLockSlim architecture to provide:
- Strong Consistency: Ranking updates are reflected immediately upon completion of the operation.
- High-Performance Queries: Utilizes Skip List indexing and Span properties to calculate rankings in $O(\log N)$ time.
- High Throughput: Implements a ReaderWriterLockSlim to support concurrent reads, optimizing for typical read-heavy leaderboard workloads.

# Architecture
The overall architecture is as follows:

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

Key ideas:
- Skip List Span: Each node stores the "distance" (span) to the next node at every level. This allows the system to calculate an element's absolute rank in $O(\log N)$ time by summing the spans during traversal.
- Read/Write Locking: A pessimistic locking strategy that allows multiple simultaneous readers but ensures exclusive access for writers during the atomic update cycle.
- Score Map: Uses a Dictionary<ulong, decimal> to store current scores, enabling $O(1)$ lookup to find the specific score required to locate and remove a node from the Skip List.

# Time Complexity
| Operation              | Time Complexity | Description                    |
| ---------------------- | --------------- | ------------------------------ |
| UpdateScore            | **O(log N)**        | Update a user's score          |
| GetLeaderboard         | **O(log N)**        | Get leaderboard range          |
| GetCustomerLeaderboard | **O(log N + K)**        | Get a user's nearby ranking    | 

# JMeter Benchmark
Test environment:
```
CPU: 8 cores / 16 threads
Memory: 64GB
.NET: .NET 10
OS: Linux
Initial data: 500k customers, 500k leaderboard entries
```

Single-endpoint test results:
![](img/single_update_customer_score.png)
![](img/single_leader_board.png)
![](img/single_leaderboard_customer.png)

Aggregate test results:
![](img/agg.png)