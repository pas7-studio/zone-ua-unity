using NUnit.Framework;
using ZoneUA.Infrastructure;

namespace ZoneUA.Combat.Tests
{
    public sealed class PoolLeaseStateTests
    {
        [Test]
        public void Acquire_IncrementsGenerationAndMarksLeaseActive()
        {
            var state = new PoolLeaseState();

            int first = state.Acquire();
            int second = state.Acquire();

            Assert.That(first, Is.EqualTo(1));
            Assert.That(second, Is.EqualTo(2));
            Assert.That(state.IsLeased, Is.True);
        }

        [Test]
        public void Release_CannotSucceedTwice()
        {
            var state = new PoolLeaseState();
            state.Acquire();

            Assert.That(state.TryRelease(), Is.True);
            Assert.That(state.TryRelease(), Is.False);
            Assert.That(state.IsLeased, Is.False);
        }

        [Test]
        public void DelayedRelease_RejectsStaleGenerationAfterReuse()
        {
            var state = new PoolLeaseState();
            int oldGeneration = state.Acquire();
            Assert.That(state.TryRelease(oldGeneration), Is.True);

            int currentGeneration = state.Acquire();

            Assert.That(state.TryRelease(oldGeneration), Is.False);
            Assert.That(state.IsCurrent(currentGeneration), Is.True);
            Assert.That(state.IsLeased, Is.True);
        }

        [Test]
        public void MatchingGeneration_ReleasesCurrentLease()
        {
            var state = new PoolLeaseState();
            int generation = state.Acquire();

            Assert.That(state.TryRelease(generation), Is.True);
            Assert.That(state.IsLeased, Is.False);
        }

        [Test]
        public void Reset_ClearsGenerationAndLeaseState()
        {
            var state = new PoolLeaseState();
            state.Acquire();

            state.Reset();

            Assert.That(state.Generation, Is.Zero);
            Assert.That(state.IsLeased, Is.False);
        }
    }
}
