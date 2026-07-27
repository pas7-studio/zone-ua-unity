namespace ZoneUA.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void ReceiveDamage(in DamageInfo damageInfo);
    }
}
