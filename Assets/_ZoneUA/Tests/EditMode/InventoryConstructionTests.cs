using System.Collections.Generic;
using NUnit.Framework;
using ZoneUA.Inventory;

namespace ZoneUA.Combat.Tests
{
    public sealed class InventoryConstructionTests
    {
        [Test]
        public void Inventory_AddRemoveAndCapacity_AreDeterministic()
        {
            var state = new InventoryState(5);
            Assert.That(state.Add("wood", 3), Is.True);
            Assert.That(state.Add("stone", 2), Is.True);
            Assert.That(state.Add("metal", 1), Is.False);
            Assert.That(state.Remove("wood", 2), Is.True);
            Assert.That(state.GetAmount("wood"), Is.EqualTo(1));
            Assert.That(state.TotalItemCount, Is.EqualTo(3));
        }

        [Test]
        public void Inventory_TryConsume_IsAtomic()
        {
            var state = new InventoryState();
            state.Add("wood", 3);
            state.Add("stone", 1);

            bool failed = state.TryConsume(new[]
            {
                new InventoryEntry("wood", 2),
                new InventoryEntry("stone", 2)
            });

            Assert.That(failed, Is.False);
            Assert.That(state.GetAmount("wood"), Is.EqualTo(3));
            Assert.That(state.GetAmount("stone"), Is.EqualTo(1));

            Assert.That(state.TryConsume(new[]
            {
                new InventoryEntry("wood", 2),
                new InventoryEntry("stone", 1)
            }), Is.True);
            Assert.That(state.GetAmount("wood"), Is.EqualTo(1));
            Assert.That(state.GetAmount("stone"), Is.Zero);
        }

        [Test]
        public void Inventory_Replace_MergesDuplicateIds()
        {
            var state = new InventoryState();
            state.Replace(new List<InventoryEntry>
            {
                new InventoryEntry("wood", 2),
                new InventoryEntry("wood", 3),
                new InventoryEntry("stone", 1)
            });

            Assert.That(state.GetAmount("wood"), Is.EqualTo(5));
            Assert.That(state.DistinctItemCount, Is.EqualTo(2));
        }

        [Test]
        public void Construction_RequiresCommittedResourcesBeforeWork()
        {
            var state = new ConstructionState(10f);
            Assert.That(state.ApplyWork(5f), Is.Zero);
            Assert.That(state.CommitResources(), Is.True);
            Assert.That(state.ApplyWork(5f), Is.EqualTo(5f));
            Assert.That(state.Progress01, Is.EqualTo(0.5f));
            Assert.That(state.ApplyWork(20f), Is.EqualTo(5f));
            Assert.That(state.IsComplete, Is.True);
        }

        [Test]
        public void Construction_Restore_ClampsProgress()
        {
            var state = new ConstructionState(1f);
            state.Restore(8f, 50f, true);
            Assert.That(state.RequiredWork, Is.EqualTo(8f));
            Assert.That(state.AppliedWork, Is.EqualTo(8f));
            Assert.That(state.IsComplete, Is.True);
        }
    }
}
