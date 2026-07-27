using UnityEngine;

namespace ZoneUA.Combat
{
    public readonly struct DamageRoll
    {
        public DamageRoll(int amount, bool isCritical)
        {
            Amount = Mathf.Max(0, amount);
            IsCritical = isCritical;
        }

        public int Amount { get; }
        public bool IsCritical { get; }
    }

    public static class ProjectileDamageCalculator
    {
        public static DamageRoll Roll(ProjectileDefinition definition)
        {
            if (definition == null)
            {
                return new DamageRoll(0, false);
            }

            return Calculate(
                definition,
                Random.value,
                Random.value);
        }

        public static DamageRoll Calculate(
            ProjectileDefinition definition,
            float damageRoll,
            float criticalRoll)
        {
            if (definition == null)
            {
                return new DamageRoll(0, false);
            }

            float clampedDamageRoll = Mathf.Clamp01(damageRoll);
            int amount = Mathf.RoundToInt(Mathf.Lerp(
                definition.MinimumDamage,
                definition.MaximumDamage,
                clampedDamageRoll));

            bool isCritical = Mathf.Clamp01(criticalRoll) < definition.CriticalChance;
            if (isCritical)
            {
                amount = Mathf.RoundToInt(amount * definition.CriticalMultiplier);
            }

            return new DamageRoll(amount, isCritical);
        }
    }
}
