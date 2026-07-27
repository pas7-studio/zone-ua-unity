namespace ZoneUA.Factions
{
    public static class FactionDamagePolicy
    {
        public static bool CanDamage(
            bool sameFaction,
            bool allowFriendlyFire,
            FactionRelation relation)
        {
            if (sameFaction)
            {
                return allowFriendlyFire;
            }

            return relation == FactionRelation.Hostile;
        }
    }
}
