using System;
using System.Collections.Generic;

namespace ZoneUA.Inventory
{
    public enum LootSourceKind
    {
        WorldItem = 0,
        Container = 1,
        Corpse = 2
    }

    public enum LootSearchState
    {
        Unsearched = 0,
        Searched = 1,
        Empty = 2
    }

    public interface ILootSource
    {
        string LootSourceId { get; }
        LootSourceKind Kind { get; }
        LootSearchState SearchState { get; }
        bool IsAvailable { get; }
        IReadOnlyList<InventoryEntry> Entries { get; }
        void MarkSearched();
        bool TryTake(InventoryState destination, string itemId, int amount);
    }

    [Serializable]
    public sealed class LootContainerState
    {
        private readonly InventoryState inventory;
        private LootSearchState searchState;

        public LootContainerState(int capacity = 0, IEnumerable<InventoryEntry> initialItems = null, bool startsSearched = false)
        {
            inventory = new InventoryState(capacity);
            inventory.Replace(initialItems);
            searchState = startsSearched ? ResolveVisibleState() : LootSearchState.Unsearched;
        }

        public InventoryState Inventory => inventory;
        public LootSearchState SearchState => searchState;
        public bool IsAvailable => inventory.TotalItemCount > 0;
        public IReadOnlyList<InventoryEntry> Entries => inventory.Entries;

        public void MarkSearched() => searchState = ResolveVisibleState();

        public bool TryTake(InventoryState destination, string itemId, int amount)
        {
            if (searchState == LootSearchState.Unsearched || destination == null) return false;
            bool transferred = inventory.TryTransferTo(destination, itemId, amount);
            if (transferred) searchState = ResolveVisibleState();
            return transferred;
        }

        public void Restore(IEnumerable<InventoryEntry> entries, LootSearchState restoredState)
        {
            inventory.Replace(entries);
            searchState = inventory.TotalItemCount == 0
                ? LootSearchState.Empty
                : restoredState == LootSearchState.Unsearched ? LootSearchState.Unsearched : LootSearchState.Searched;
        }

        private LootSearchState ResolveVisibleState() => inventory.TotalItemCount == 0 ? LootSearchState.Empty : LootSearchState.Searched;
    }
}
