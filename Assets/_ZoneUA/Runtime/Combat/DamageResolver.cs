using System;

namespace ZoneUA.Combat
{
    public readonly struct DamageResolution
    {
        public DamageResolution(float rawAmount, float resistance, int appliedAmount)
        {
            RawAmount = Math.Max(0f, rawAmount);
            Resistance = DamageResolver.ClampResistance(resistance);
            AppliedAmount = Math.Max(0, appliedAmount);
        }

        public float RawAmount { get; }
        public float Resistance { get; }
        public int AppliedAmount { get; }
        public bool WasBlocked => RawAmount > 0f && AppliedAmount == 0;
    }

    public static class DamageResolver
    {
        public static DamageResolution Resolve(float rawAmount, float resistance, float multiplier = 1f)
        {
            float safeRaw = Math.Max(0f, rawAmount);
            float safeResistance = ClampResistance(resistance);
            float safeMultiplier = Math.Max(0f, multiplier);
            float reduced = safeRaw * (1f - safeResistance) * safeMultiplier;
            int applied = reduced <= 0f ? 0 : Math.Max(1, (int)Math.Ceiling(reduced));
            return new DamageResolution(safeRaw, safeResistance, applied);
        }

        internal static float ClampResistance(float value)
        {
            return Math.Min(1f, Math.Max(-1f, value));
        }
    }
}
