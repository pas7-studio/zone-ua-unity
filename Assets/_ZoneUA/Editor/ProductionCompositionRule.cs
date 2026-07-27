using System;
using System.Collections.Generic;

namespace ZoneUA.EditorValidation
{
    [Serializable]
    public sealed class ProductionCompositionRule
    {
        public string Name { get; }
        public string AnchorTypeName { get; }
        public bool CanAutoAdd { get; }
        public IReadOnlyList<string> RequiredTypeNames { get; }

        public ProductionCompositionRule(
            string name,
            string anchorTypeName,
            bool canAutoAdd,
            params string[] requiredTypeNames)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AnchorTypeName = anchorTypeName ?? throw new ArgumentNullException(nameof(anchorTypeName));
            CanAutoAdd = canAutoAdd;
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
                true,
                "Health",
                "Death",
                "PlayerInputRouter",
                "WeaponSwitcher"),
            new ProductionCompositionRule(
                "NPC actor",
                "NPCController",
                true,
                "Health",
                "Death",
                "ZoneUA.Factions.FactionMember"),
            new ProductionCompositionRule(
                "Weapon",
                "WeaponController",
                true,
                "Weapon",
                "ZoneUA.Combat.ProjectileSpawner",
                "ZoneUA.Combat.ShellEjector",
                "ZoneUA.Combat.WeaponAudio",
                "ZoneUA.Combat.WeaponRecoil"),
            new ProductionCompositionRule(
                "Damage presentation",
                "Health",
                false,
                "DamageEffectsPresenter"),
            new ProductionCompositionRule(
                "World root",
                "MapGenerator",
                true,
                "ChunkManager"),
            new ProductionCompositionRule(
                "Ammo HUD",
                "UIAmmoSystem",
                false,
                "WeaponAmmoPresenter")
        };

        public static IReadOnlyList<ProductionCompositionRule> Rules => RulesInternal;
    }
}
