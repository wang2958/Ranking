namespace Ranking.Api.Collections
{
    public class SkipList
    {
        const int MaxHeight = 16;
        const double P = 0.25;

        Random _random = Random.Shared;

        int Height = 1;
        int CurrentLength = 0;
        public int Length => CurrentLength;

        SkipListNode Head;

        public SkipList()
        {
            Head = new SkipListNode(0, 0, MaxHeight);
        }

        int RandomHeight()
        {
            int height = 1;

            while (_random.NextDouble() < P && height < MaxHeight)
            {
                height++;
            }
            return height;
        }

        public void Insert(ulong id, decimal score)
        {
            var update = new SkipListNode[MaxHeight];
            var rankList = new int[MaxHeight];
            var currentNode = Head;

            for (int i = Height - 1; i >= 0; i--)
            {
                rankList[i] = (i == Height - 1) ? 0 : rankList[i + 1];
                while (currentNode.Next[i] != null && currentNode.Next[i].CompareTo(id, score) < 0)
                {
                    rankList[i] += currentNode.Span[i];
                    currentNode = currentNode.Next[i];
                }
                update[i] = currentNode;
            }

            int randomHeight = RandomHeight();
            if (randomHeight > Height)
            {
                for (int i = Height; i < randomHeight; i++)
                {
                    rankList[i] = 0;
                    update[i] = Head;
                    update[i].Span[i] = CurrentLength;
                }
                Height = randomHeight;
            }

            var newNode = new SkipListNode(id, score, randomHeight);
            for (int i = 0; i < randomHeight; i++)
            {
                newNode.Next[i] = update[i].Next[i];
                update[i].Next[i] = newNode;

                newNode.Span[i] = update[i].Span[i] - (rankList[0] - rankList[i]);
                update[i].Span[i] = rankList[0] - rankList[i] + 1;
            }

            for (int i = randomHeight; i < Height; i++)
            {
                update[i].Span[i]++;
            }

            CurrentLength++;
        }

        public void Delete(ulong customerId, decimal score)
        {
            var update = new SkipListNode[MaxHeight];
            var currentNode = Head;

            for (int i = Height - 1; i >= 0; i--)
            {
                while (currentNode.Next[i] != null && currentNode.Next[i].CompareTo(customerId, score) < 0)
                {
                    currentNode = currentNode.Next[i];
                }
                update[i] = currentNode;
            }

            currentNode = currentNode.Next[0];

            if (currentNode == null || currentNode.CustomerId != customerId || currentNode.Score != score) return;

            for (int i = 0; i < Height; i++)
            {
                if (update[i].Next[i] == currentNode)
                {
                    update[i].Span[i] += currentNode.Span[i] - 1;
                    update[i].Next[i] = currentNode.Next[i];
                }
                else
                {
                    update[i].Span[i]--;
                }
            }

            while (Height > 1 && Head.Next[Height - 1] == null)
            {
                Height--;
            }

            CurrentLength--;
        }

        public int GetRank(ulong customerId, decimal score)
        {
            int currentRank = 0;
            var currentNode = Head;

            for (int i = Height - 1; i >= 0; i--)
            {
                while (currentNode.Next[i] != null && currentNode.Next[i].CompareTo(customerId, score) <= 0)
                {
                    currentRank += currentNode.Span[i];
                    currentNode = currentNode.Next[i];
                }

                if (currentNode.CustomerId == customerId)
                {
                    return currentRank;
                }
            }
            return -1;
        }

        public List<SkipListNode> GetRange(int start, int end)
        {
            var result = new List<SkipListNode>();
            if (start < 1) start = 1;

            int currentRank = 0;
            var currentNode = Head;

            for (int i = Height - 1; i >= 0; i--)
            {
                while (currentNode.Next[i] != null && currentRank + currentNode.Span[i] < start)
                {
                    currentRank += currentNode.Span[i];
                    currentNode = currentNode.Next[i];
                }
            }

            currentNode = currentNode.Next[0];
            currentRank++;

            while (currentNode != null && currentRank <= end)
            {
                result.Add(currentNode);
                currentNode = currentNode.Next[0];
                currentRank++;
            }

            return result;
        }
    }
}