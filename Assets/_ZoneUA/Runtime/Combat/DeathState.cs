namespace ZoneUA.Combat
{
    public sealed class DeathState
    {
        public bool IsDead { get; private set; }

        public bool TryEnter()
        {
            if (IsDead)
            {
                return false;
            }

            IsDead = true;
            return true;
        }

        public void Reset()
        {
            IsDead = false;
        }
    }
}
