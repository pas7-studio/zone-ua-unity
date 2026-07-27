namespace ZoneUA.Infrastructure
{
    public sealed class PoolLeaseState
    {
        private int generation;
        private bool isLeased;

        public int Generation => generation;
        public bool IsLeased => isLeased;

        public int Acquire()
        {
            generation = generation == int.MaxValue ? 1 : generation + 1;
            isLeased = true;
            return generation;
        }

        public bool TryRelease()
        {
            if (!isLeased)
            {
                return false;
            }

            isLeased = false;
            return true;
        }

        public bool TryRelease(int expectedGeneration)
        {
            if (!isLeased || expectedGeneration != generation)
            {
                return false;
            }

            isLeased = false;
            return true;
        }

        public bool IsCurrent(int expectedGeneration)
        {
            return isLeased && expectedGeneration == generation;
        }

        public void Reset()
        {
            generation = 0;
            isLeased = false;
        }
    }
}
