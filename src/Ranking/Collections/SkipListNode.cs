namespace Ranking.Api.Collections
{
    public class SkipListNode : IComparable<SkipListNode>
    {
        public ulong CustomerId;
        public decimal Score;

        public SkipListNode[] Next;
        public int[] Span;

        public SkipListNode(ulong id, decimal score, int height)
        {
            CustomerId = id;
            Score = score;
            Next = new SkipListNode[height];
            Span = new int[height];
        }

        public int CompareTo(SkipListNode? other)
        {
            return CompareTo(other?.CustomerId ?? 0, other?.Score ?? 0);
        }

        public int CompareTo(ulong id, decimal score)
        {
            int cmp = score.CompareTo(this.Score);

            if (cmp != 0) return cmp;

            return this.CustomerId.CompareTo(id);
        }
    }
}