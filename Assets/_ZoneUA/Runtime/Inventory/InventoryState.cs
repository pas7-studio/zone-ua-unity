using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneUA.Inventory
{
    [Serializable]
    public sealed class InventoryEntry
    {
        public string itemId = string.Empty;
        public int amount;

        public InventoryEntry() { }
        public InventoryEntry(string itemId, int amount)
        {
            this.itemId = itemId ?? string.Empty;
            this.amount = Math.Max(0, amount);
        }
    }

    public sealed class InventoryState
    {
        private readonly Dictionary<string, int> amounts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly int capacity;

        public InventoryState(int capacity = 0) => this.capacity = Math.Max(0, capacity);
        public int Capacity => capacity;
        public int DistinctItemCount => amounts.Count;
        public int TotalItemCount => amounts.Values.Sum();
        public IReadOnlyList<InventoryEntry> Entries => amounts.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => new InventoryEntry(pair.Key, pair.Value)).ToList();

        public int GetAmount(string itemId) => Normalize(itemId) is string id && amounts.TryGetValue(id, out int value) ? value : 0;

        public bool CanAdd(string itemId, int amount)
        {
            string id = Normalize(itemId);
            if (string.IsNullOrEmpty(id) || amount <= 0) return false;
            return capacity <= 0 || TotalItemCount + amount <= capacity;
        }

        public bool Add(string itemId, int amount)
        {
            string id = Normalize(itemId);
            if (!CanAdd(id, amount)) return false;
            amounts[id] = GetAmount(id) + amount;
            return true;
        }

        public bool Has(string itemId, int amount = 1) => amount > 0 && GetAmount(itemId) >= amount;

        public bool Remove(string itemId, int amount)
        {
            string id = Normalize(itemId);
            if (string.IsNullOrEmpty(id) || amount <= 0 || !amounts.TryGetValue(id, out int current) || current < amount) return false;
            int remaining = current - amount;
            if (remaining == 0) amounts.Remove(id); else amounts[id] = remaining;
            return true;
        }

        public bool TryConsume(IEnumerable<InventoryEntry> costs)
        {
            List<InventoryEntry> normalized = NormalizeEntries(costs);
            if (normalized.Any(entry => !Has(entry.itemId, entry.amount))) return false;
            foreach (InventoryEntry entry in normalized) Remove(entry.itemId, entry.amount);
            return true;
        }

        public void Replace(IEnumerable<InventoryEntry> entries)
        {
            amounts.Clear();
            foreach (InventoryEntry entry in NormalizeEntries(entries))
            {
                if (capacity > 0 && TotalItemCount + entry.amount > capacity) break;
                amounts[entry.itemId] = GetAmount(entry.itemId) + entry.amount;
            }
        }

        private static List<InventoryEntry> NormalizeEntries(IEnumerable<InventoryEntry> entries)
        {
            return (entries ?? Array.Empty<InventoryEntry>())
                .Where(entry => entry != null && entry.amount > 0 && !string.IsNullOrWhiteSpace(entry.itemId))
                .GroupBy(entry => entry.itemId.Trim(), StringComparer.Ordinal)
                .Select(group => new InventoryEntry(group.Key, group.Sum(entry => entry.amount)))
                .OrderBy(entry => entry.itemId, StringComparer.Ordinal)
                .ToList();
        }

        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
