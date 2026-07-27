using UnityEngine;

namespace ZoneUA.Factions
{
    [DisallowMultipleComponent]
    public sealed class FactionMember : MonoBehaviour
    {
        [SerializeField, Tooltip("Faction used by targeting and friendly-fire rules.")]
        private FactionDefinition faction;

        public FactionDefinition Faction => faction;

        public bool IsFriendlyTo(FactionMember other)
        {
            return GetRelationTo(other) == FactionRelation.Friendly;
        }

        public bool IsHostileTo(FactionMember other)
        {
            return GetRelationTo(other) == FactionRelation.Hostile;
        }

        public FactionRelation GetRelationTo(FactionMember other)
        {
            if (faction == null)
            {
                return FactionRelation.Neutral;
            }

            return faction.GetRelationTo(other != null ? other.faction : null);
        }

        public bool CanDamage(FactionMember other)
        {
            if (other == null || faction == null || other.faction == null)
            {
                return true;
            }

            bool sameFaction = faction == other.faction;
            FactionRelation relation = sameFaction
                ? FactionRelation.Friendly
                : faction.GetRelationTo(other.faction);

            return FactionDamagePolicy.CanDamage(
                sameFaction,
                faction.AllowFriendlyFire,
                relation);
        }
    }
}
