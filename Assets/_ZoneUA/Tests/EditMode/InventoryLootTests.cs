using System.Collections.Generic;
using NUnit.Framework;
using ZoneUA.Inventory;

namespace ZoneUA.Combat.Tests
{
    public sealed class InventoryLootTests
    {
        [Test]
        public void Inventory_AddRemoveAndCapacity_AreDeterministic()
        {
            var state = new InventoryState(5);
            Assert.That(state.Add("ammo-9x19", 3), Is.True);
            Assert.That(state.Add("medkit", 2), Is.True);
            Assert.That(state.Add("food", 1), Is.False);
            Assert.That(state.Remove("ammo-9x19", 2), Is.True);
            Assert.That(state.GetAmount("ammo-9x19"), Is.EqualTo(1));
            Assert.That(state.TotalItemCount, Is.EqualTo(3));
        }

        [Test]
        public void Inventory_TryConsume_IsAtomic()
        {
            var state = new InventoryState();
            state.Add("ammo-9x19", 3);
            state.Add("medkit", 1);

            bool failed = state.TryConsume(new[]
            {
                new InventoryEntry("ammo-9x19", 2),
                new InventoryEntry("medkit", 2)
            });

            Assert.That(failed, Is.False);
            Assert.That(state.GetAmount("ammo-9x19"), Is.EqualTo(3));
            Assert.That(state.GetAmount("medkit"), Is.EqualTo(1));
        }

        [Test]
        public void Inventory_Transfer_IsAtomicWhenDestinationIsFull()
        {
            var source = new InventoryState();
            var destination = new InventoryState(1);
            source.Add("medkit", 2);
            destination.Add("ammo-9x19", 1);

            Assert.That(source.TryTransferTo(destination, "medkit", 1), Is.False);
            Assert.That(source.GetAmount("medkit"), Is.EqualTo(2));
            Assert.That(destination.GetAmount("medkit"), Is.Zero);
        }

        [Test]
        public void LootContainer_MustBeSearchedBeforeTransfer()
        {
            var loot = new LootContainerState(initialItems: new[] { new InventoryEntry("medkit", 1) });
            var destination = new InventoryState();

            Assert.That(loot.TryTake(destination, "medkit", 1), Is.False);
            loot.MarkSearched();
            Assert.That(loot.TryTake(destination, "medkit", 1), Is.True);
            Assert.That(loot.SearchState, Is.EqualTo(LootSearchState.Empty));
            Assert.That(destination.GetAmount("medkit"), Is.EqualTo(1));
        }

        [Test]
        public void Reservation_RejectsOtherNpcUntilExpiry()
        {
            var registry = new LootReservationRegistry();
            Assert.That(registry.TryReserve("corpse-1", "npc-a", 10d, 5d), Is.True);
            Assert.That(registry.TryReserve("corpse-1", "npc-b", 11d, 5d), Is.False);
            Assert.That(registry.IsReservedByOther("corpse-1", "npc-b", 11d), Is.True);
            Assert.That(registry.TryReserve("corpse-1", "npc-b", 16d, 5d), Is.True);
        }

        [Test]
        public void Inventory_Replace_MergesDuplicateIds()
        {
            var state = new InventoryState();
            state.Replace(new List<InventoryEntry>
            {
                new InventoryEntry("ammo-9x19", 2),
                new InventoryEntry("ammo-9x19", 3),
                new InventoryEntry("medkit", 1)
            });

            Assert.That(state.GetAmount("ammo-9x19"), Is.EqualTo(5));
            Assert.That(state.DistinctItemCount, Is.EqualTo(2));
        }
    }
}
