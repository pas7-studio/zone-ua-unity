using NUnit.Framework;
using ZoneUA.AI;

namespace ZoneUA.Combat.Tests
{
    public sealed class NpcBrainTests
    {
        [Test]
        public void AliveNpcWithoutTargetPatrolsWhenPatrolExists()
        {
            var brain = new NpcBrainState();
            NpcState state = brain.Evaluate(true, 1f, 0.2f, true, float.PositiveInfinity, 5f, 2f, 0f);
            Assert.That(state, Is.EqualTo(NpcState.Patrol));
        }

        [Test]
        public void VisibleTargetInsideAttackDistanceTriggersAttack()
        {
            var brain = new NpcBrainState();
            brain.SetTargetVisible(true, 1f);
            NpcState state = brain.Evaluate(true, 1f, 0.2f, true, 3f, 5f, 2f, 1f);
            Assert.That(state, Is.EqualTo(NpcState.Attack));
        }

        [Test]
        public void LowHealthNpcWithTargetFlees()
        {
            var brain = new NpcBrainState();
            brain.SetTargetVisible(true, 1f);
            NpcState state = brain.Evaluate(true, 0.1f, 0.2f, true, 3f, 5f, 2f, 1f);
            Assert.That(state, Is.EqualTo(NpcState.Flee));
        }

        [Test]
        public void RecentlyLostTargetRemainsInChase()
        {
            var brain = new NpcBrainState();
            brain.SetTargetVisible(true, 1f);
            brain.SetTargetVisible(false, 1.5f);
            NpcState state = brain.Evaluate(true, 1f, 0.2f, true, float.PositiveInfinity, 5f, 3f, 3f);
            Assert.That(state, Is.EqualTo(NpcState.Chase));
        }

        [Test]
        public void DeadStateIsTerminal()
        {
            var brain = new NpcBrainState();
            Assert.That(brain.Transition(NpcState.Dead), Is.True);
            Assert.That(brain.Transition(NpcState.Patrol), Is.False);
            Assert.That(brain.Current, Is.EqualTo(NpcState.Dead));
        }

        [Test]
        public void TargetScoringRejectsNonHostileOrInvisibleCandidates()
        {
            Assert.That(float.IsNegativeInfinity(NpcTargetScoring.Score(1f, false, true, true)), Is.True);
            Assert.That(float.IsNegativeInfinity(NpcTargetScoring.Score(1f, true, true, false)), Is.True);
        }

        [Test]
        public void TargetScoringPrefersNearestValidTarget()
        {
            float near = NpcTargetScoring.Score(4f, true, true, true);
            float far = NpcTargetScoring.Score(25f, true, true, true);
            Assert.That(NpcTargetScoring.IsBetter(near, far), Is.True);
        }
    }
}
