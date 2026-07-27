using NUnit.Framework;
using ZoneUA.SceneManagement;

namespace ZoneUA.Combat.Tests
{
    public sealed class SceneTransitionStateTests
    {
        [Test]
        public void TryBegin_ValidTarget_EntersLoading()
        {
            var state = new SceneTransitionState();
            Assert.That(state.TryBegin("Old", "New"), Is.True);
            Assert.That(state.Phase, Is.EqualTo(SceneTransitionPhase.Loading));
            Assert.That(state.CurrentScene, Is.EqualTo("Old"));
            Assert.That(state.TargetScene, Is.EqualTo("New"));
        }

        [Test]
        public void TryBegin_WhileBusy_IsRejected()
        {
            var state = new SceneTransitionState();
            state.TryBegin("Old", "New");
            Assert.That(state.TryBegin("Old", "Other"), Is.False);
        }

        [Test]
        public void Progress_IsClamped()
        {
            var state = new SceneTransitionState();
            state.TryBegin("", "New");
            state.SetProgress(2f);
            Assert.That(state.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void Complete_PromotesTargetToCurrent()
        {
            var state = new SceneTransitionState();
            state.TryBegin("Old", "New");
            state.Complete();
            Assert.That(state.Phase, Is.EqualTo(SceneTransitionPhase.Completed));
            Assert.That(state.CurrentScene, Is.EqualTo("New"));
            Assert.That(state.TargetScene, Is.Empty);
        }

        [Test]
        public void Fail_ClearsTargetAndStoresError()
        {
            var state = new SceneTransitionState();
            state.TryBegin("Old", "New");
            state.Fail("boom");
            Assert.That(state.Phase, Is.EqualTo(SceneTransitionPhase.Failed));
            Assert.That(state.TargetScene, Is.Empty);
            Assert.That(state.Error, Is.EqualTo("boom"));
        }
    }
}
