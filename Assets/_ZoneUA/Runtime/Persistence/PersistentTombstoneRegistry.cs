using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneUA.Persistence
{
    public static class PersistentTombstoneRegistry
    {
        private static readonly HashSet<string> Tombstones = new HashSet<string>(StringComparer.Ordinal);

        public static IReadOnlyCollection<string> Current => Tombstones;

        public static bool MarkDestroyed(string objectId) =>
            !string.IsNullOrWhiteSpace(objectId) && Tombstones.Add(objectId.Trim());

        public static bool Remove(string objectId) =>
            !string.IsNullOrWhiteSpace(objectId) && Tombstones.Remove(objectId.Trim());

        public static void Replace(IEnumerable<string> objectIds)
        {
            Tombstones.Clear();
            if (objectIds == null) return;
            foreach (string id in objectIds.Where(value => !string.IsNullOrWhiteSpace(value))) Tombstones.Add(id.Trim());
        }

        public static void Clear() => Tombstones.Clear();
    }
}