using System;
using System.Collections.Generic;

namespace ZoneUA.EditorValidation
{
    [Serializable]
    public sealed class ProductionCompositionRule
    {
        public string Name { get; }
        public string AnchorTypeName { get; }
        public IReadOnlyList<string> RequiredTypeNames { get; }

        public ProductionCompositionRule(string name, string anchorTypeName, params string[] requiredTypeNames)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AnchorTypeName = anchorTypeName ?? throw new ArgumentNullException(nameof(anchorTypeName));
            RequiredTypeNames = requiredTypeNames ?? Array.Empty<string>();
        }
    }

    public static class ProductionCompositionCatalog
    {
        private static readonly ProductionCompositionRule[] RulesInternal =
        {
            new ProductionCompositionRule(
                "Player root",
                "CharacterCustomController",
                "PlayerInputRouter",
                "WeaponSwitcher"),
            new ProductionCompositionRule(
                "NPC actor",
                "NPCController",
                "Health",
                "Death",
                "ZoneUA.Factions.FactionMember"),
            new ProductionCompositionRule(
                "Weapon",
                "WeaponController",
                "Weapon",
                "ZoneUA.Combat.ProjectileSpawner",
                "ZoneUA.Combat.ShellEjector",
                "ZoneUA.Combat.WeaponAudio",
                "ZoneUA.Combat.WeaponRecoil"),
            new ProductionCompositionRule(
                "Damageable actor",
                "Health",
                "Death",
                "DamageEffectsPresenter"),
            new ProductionCompositionRule(
                "World root",
                "MapGenerator",
                "ChunkManager"),
            new ProductionCompositionRule(
                "Ammo HUD",
                "UIAmmoSystem",
                "WeaponAmmoPresenter")
        };

        public static IReadOnlyList<ProductionCompositionRule> Rules => RulesInternal;
    }
}
