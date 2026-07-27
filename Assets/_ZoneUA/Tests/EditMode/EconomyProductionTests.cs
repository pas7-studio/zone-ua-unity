using NUnit.Framework;
using ZoneUA.Economy;

namespace ZoneUA.Combat.Tests
{
    public sealed class EconomyProductionTests
    {
        [Test]
        public void HarvestState_AccumulatesWorkAndDepletesDeterministically()
        {
            var state = new HarvestState(5);

            Assert.That(state.ApplyWork(0.5f, 1f, 2), Is.EqualTo(0));
            Assert.That(state.ApplyWork(0.5f, 1f, 2), Is.EqualTo(2));
            Assert.That(state.remainingUnits, Is.EqualTo(3));
            Assert.That(state.ApplyWork(2f, 1f, 2), Is.EqualTo(3));
            Assert.That(state.IsDepleted, Is.True);
        }

        [Test]
        public void HarvestState_RestoreClampsInvalidValues()
        {
            var state = new HarvestState(10);
            state.Restore(-10, -5f, -3f);

            Assert.That(state.remainingUnits, Is.Zero);
            Assert.That(state.accumulatedWork, Is.Zero);
            Assert.That(state.respawnRemaining, Is.Zero);
        }

        [Test]
        public void ProductionQueue_AdvancesOneCycleAtATime()
        {
            var queue = new ProductionQueueState();
            queue.Enqueue("plank", 2);

            Assert.That(queue.TryAdvance(0.5f, 1f, out _), Is.False);
            Assert.That(queue.TryAdvance(0.5f, 1f, out string first), Is.True);
            Assert.That(first, Is.EqualTo("plank"));
            Assert.That(queue.entries[0].remainingCycles, Is.EqualTo(1));
            Assert.That(queue.TryAdvance(1f, 1f, out string second), Is.True);
            Assert.That(second, Is.EqualTo("plank"));
            Assert.That(queue.entries, Is.Empty);
        }

        [Test]
        public void ProductionQueue_NormalizeRemovesInvalidEntries()
        {
            var queue = new ProductionQueueState();
            queue.entries.Add(new ProductionQueueEntry { recipeId = "", remainingCycles = 1 });
            queue.entries.Add(new ProductionQueueEntry { recipeId = " valid ", remainingCycles = 2, elapsed = -1f });

            queue.Normalize();

            Assert.That(queue.entries.Count, Is.EqualTo(1));
            Assert.That(queue.entries[0].recipeId, Is.EqualTo("valid"));
            Assert.That(queue.entries[0].elapsed, Is.Zero);
        }

        [Test]
        public void WorkerJobState_AssignAndClearAreExplicit()
        {
            var state = new WorkerJobState();
            state.Assign(WorkerJobKind.Harvest, " node-1 ");

            Assert.That(state.kind, Is.EqualTo(WorkerJobKind.Harvest));
            Assert.That(state.targetObjectId, Is.EqualTo("node-1"));

            state.Clear();
            Assert.That(state.kind, Is.EqualTo(WorkerJobKind.None));
            Assert.That(state.targetObjectId, Is.Empty);
        }
    }
}
