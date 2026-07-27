namespace ZoneUA.World
{
    public static class WorldDeterminism
    {
        public static float Value01(int seed, int x, int y, int salt = 0)
        {
            unchecked
            {
                uint hash = (uint)seed;
                hash ^= (uint)x * 0x9E3779B9u;
                hash = (hash << 13) | (hash >> 19);
                hash ^= (uint)y * 0x85EBCA6Bu;
                hash = (hash << 11) | (hash >> 21);
                hash ^= (uint)salt * 0xC2B2AE35u;
                hash ^= hash >> 16;
                hash *= 0x7FEB352Du;
                hash ^= hash >> 15;
                hash *= 0x846CA68Bu;
                hash ^= hash >> 16;
                return (hash & 0x00FFFFFFu) / 16777215f;
            }
        }

        public static int Index(int seed, int x, int y, int count, int salt = 0)
        {
            if (count <= 0)
            {
                return -1;
            }

            int index = (int)(Value01(seed, x, y, salt) * count);
            return index >= count ? count - 1 : index;
        }
    }
}
