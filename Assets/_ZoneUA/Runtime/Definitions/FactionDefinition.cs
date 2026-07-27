using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneUA.Factions
{
    [CreateAssetMenu(fileName = "Faction", menuName = "Zone UA/Factions/Faction Definition")]
    public sealed class FactionDefinition : ScriptableObject
    {
        [Serializable]
        private struct RelationEntry
        {
            [SerializeField] private FactionDefinition faction;
            [SerializeField] private FactionRelation relation;

            public FactionDefinition Faction => faction;
            public FactionRelation Relation => relation;
        }

        [Header("Identity")]
        [SerializeField, Tooltip("Stable identifier used by saves and runtime systems.")]
        private string id = "faction";

        [SerializeField, Tooltip("Human-readable faction name.")]
        private string displayName = "Faction";

        [Header("Combat")]
        [SerializeField, Tooltip("Whether members of this faction may damage one another.")]
        private bool allowFriendlyFire;

        [SerializeField]
        private FactionRelation defaultRelation = FactionRelation.Neutral;

        [SerializeField]
        private List<RelationEntry> relations = new();

        public string Id => id;
        public string DisplayName => displayName;
        public bool AllowFriendlyFire => allowFriendlyFire;
        public FactionRelation DefaultRelation => defaultRelation;

        public FactionRelation GetRelationTo(FactionDefinition other)
        {
            if (other == null)
            {
                return defaultRelation;
            }

            if (other == this)
            {
                return allowFriendlyFire ? FactionRelation.Hostile : FactionRelation.Friendly;
            }

            for (int i = 0; i < relations.Count; i++)
            {
                RelationEntry entry = relations[i];
                if (entry.Faction == other)
                {
                    return entry.Relation;
                }
            }

            return defaultRelation;
        }

        private void OnValidate()
        {
            id = string.IsNullOrWhiteSpace(id) ? name.Trim().ToLowerInvariant().Replace(' ', '-') : id.Trim();
            displayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
        }
    }
}
