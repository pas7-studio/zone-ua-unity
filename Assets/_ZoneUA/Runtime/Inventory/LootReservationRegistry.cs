using System;
using System.Collections.Generic;

namespace ZoneUA.Inventory
{
    public sealed class LootReservationRegistry
    {
        private sealed class Reservation
        {
            public string ownerId;
            public double expiresAt;
        }

        private readonly Dictionary<string, Reservation> reservations = new Dictionary<string, Reservation>(StringComparer.Ordinal);

        public bool TryReserve(string sourceId, string ownerId, double now, double durationSeconds)
        {
            sourceId = Normalize(sourceId);
            ownerId = Normalize(ownerId);
            if (string.IsNullOrEmpty(sourceId) || string.IsNullOrEmpty(ownerId) || durationSeconds <= 0d) return false;
            PruneExpired(now);
            if (reservations.TryGetValue(sourceId, out Reservation existing) && !string.Equals(existing.ownerId, ownerId, StringComparison.Ordinal)) return false;
            reservations[sourceId] = new Reservation { ownerId = ownerId, expiresAt = now + durationSeconds };
            return true;
        }

        public bool IsReservedByOther(string sourceId, string ownerId, double now)
        {
            PruneExpired(now);
            sourceId = Normalize(sourceId);
            ownerId = Normalize(ownerId);
            return reservations.TryGetValue(sourceId, out Reservation existing)
                && !string.Equals(existing.ownerId, ownerId, StringComparison.Ordinal);
        }

        public void Release(string sourceId, string ownerId)
        {
            sourceId = Normalize(sourceId);
            ownerId = Normalize(ownerId);
            if (!reservations.TryGetValue(sourceId, out Reservation existing)) return;
            if (string.IsNullOrEmpty(ownerId) || string.Equals(existing.ownerId, ownerId, StringComparison.Ordinal)) reservations.Remove(sourceId);
        }

        public void ReleaseAll(string ownerId)
        {
            ownerId = Normalize(ownerId);
            if (string.IsNullOrEmpty(ownerId)) return;
            var keys = new List<string>();
            foreach (KeyValuePair<string, Reservation> pair in reservations)
                if (string.Equals(pair.Value.ownerId, ownerId, StringComparison.Ordinal)) keys.Add(pair.Key);
            foreach (string key in keys) reservations.Remove(key);
        }

        public void PruneExpired(double now)
        {
            var keys = new List<string>();
            foreach (KeyValuePair<string, Reservation> pair in reservations)
                if (pair.Value.expiresAt <= now) keys.Add(pair.Key);
            foreach (string key in keys) reservations.Remove(key);
        }

        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
