using UnityEngine;

namespace ZoneUA.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            int amount,
            GameObject source,
            GameObject instigator,
            Vector2 hitPoint,
            Vector2 hitDirection,
            DamageType damageType,
            bool isCritical = false)
        {
            Amount = Mathf.Max(0, amount);
            Source = source;
            Instigator = instigator;
            HitPoint = hitPoint;
            HitDirection = hitDirection.sqrMagnitude > 0f ? hitDirection.normalized : Vector2.zero;
            DamageType = damageType;
            IsCritical = isCritical;
        }

        public int Amount { get; }
        public GameObject Source { get; }
        public GameObject Instigator { get; }
        public Vector2 HitPoint { get; }
        public Vector2 HitDirection { get; }
        public DamageType DamageType { get; }
        public bool IsCritical { get; }
    }
}
