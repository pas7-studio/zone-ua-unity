using System;
using System.Collections.Generic;

namespace ZoneUA.EditorValidation
{
    public enum LegacyUsageSeverity
    {
        Info,
        Warning,
        Error
    }

    [Serializable]
    public sealed class LegacyUsageRule
    {
        public string Code { get; }
        public string Description { get; }
        public LegacyUsageSeverity Severity { get; }
        public IReadOnlyList<string> Tokens { get; }
        public IReadOnlyList<string> AllowedPathFragments { get; }

        public LegacyUsageRule(
            string code,
            string description,
            LegacyUsageSeverity severity,
            string[] tokens,
            string[] allowedPathFragments = null)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Severity = severity;
            Tokens = tokens ?? Array.Empty<string>();
            AllowedPathFragments = allowedPathFragments ?? Array.Empty<string>();
        }

        public bool IsAllowedPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            for (int i = 0; i < AllowedPathFragments.Count; i++)
            {
                if (path.IndexOf(AllowedPathFragments[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static class LegacyUsageCatalog
    {
        private static readonly LegacyUsageRule[] RulesInternal =
        {
            new LegacyUsageRule(
                "LEGACY_INPUT",
                "Gameplay code must use PlayerInputRouter/Input System instead of UnityEngine.Input polling.",
                LegacyUsageSeverity.Error,
                new[] { "Input.GetAxis", "Input.GetAxisRaw", "Input.GetButton", "Input.GetButtonDown", "Input.GetKey", "Input.GetKeyDown", "Input.mousePosition" },
                new[] { "/Editor/", "/Tests/", "LegacyUsageRule.cs" }),
            new LegacyUsageRule(
                "DIRECT_HUD_DEPENDENCY",
                "Gameplay code must publish events instead of accessing the ammo HUD directly.",
                LegacyUsageSeverity.Error,
                new[] { "GlobalSystem.Instance.AmmoUI", ".AmmoUI." },
                new[] { "/Editor/", "/Tests/" }),
            new LegacyUsageRule(
                "TAG_BASED_COMBAT",
                "Combat targeting and damage filtering must use factions rather than Player/Enemy tags.",
                LegacyUsageSeverity.Warning,
                new[] { "CompareTag(\"Player\")", "CompareTag(\"Enemy\")", "FindGameObjectWithTag(\"Player\")", "FindGameObjectsWithTag(\"Enemy\")", "whoRecieveDamage" },
                new[] { "/Editor/", "/Tests/", "Bullet.cs" }),
            new LegacyUsageRule(
                "LEGACY_HEALTH_API",
                "New code must use SetHealth, RestoreHealth, CurrentHealth, IsAlive and ReceiveDamage(DamageInfo).",
                LegacyUsageSeverity.Warning,
                new[] { ".setHeals(", ".restoreSomeHeals(", ".restoreDefaultHeals(", ".getHeals(", ".receiveDamage(", ".getIsAlive(" },
                new[] { "/Editor/", "/Tests/", "Health.cs" }),
            new LegacyUsageRule(
                "GLOBAL_FIND",
                "Runtime hot paths should use serialized references, registration or events instead of scene-wide searches.",
                LegacyUsageSeverity.Warning,
                new[] { "FindObjectOfType<", "FindObjectsOfType<", "GameObject.Find(" },
                new[] { "/Editor/", "/Tests/", "RuntimePerformanceMonitor.cs" }),
            new LegacyUsageRule(
                "LEGACY_POOL_API",
                "Use TryGetRandomBlood and typed pool methods instead of legacy aliases.",
                LegacyUsageSeverity.Info,
                new[] { ".getRandomBlood(" },
                new[] { "/Editor/", "/Tests/", "GlobalSystem.cs" })
        };

        public static IReadOnlyList<LegacyUsageRule> Rules => RulesInternal;
    }
}
