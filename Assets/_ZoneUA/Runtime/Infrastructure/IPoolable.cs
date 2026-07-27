namespace ZoneUA.Infrastructure
{
    public interface IPoolable
    {
        void OnPoolSpawned();
        void OnPoolReleased();
    }
}
