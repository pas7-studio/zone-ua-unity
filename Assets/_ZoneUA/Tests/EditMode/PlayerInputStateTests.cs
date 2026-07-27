using NUnit.Framework;
using UnityEngine;
using ZoneUA.Input;

namespace ZoneUA.Combat.Tests
{
    public sealed class PlayerInputStateTests
    {
        [Test]
        public void Move_IsClampedToUnitLength()
        {
            var state = new PlayerInputState();

            state.SetMove(new Vector2(3f, 4f));

            Assert.That(state.Move.magnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Move_DoesNotReportChangeForSameValue()
        {
            var state = new PlayerInputState();
            state.SetMove(Vector2.right);

            Assert.That(state.SetMove(Vector2.right), Is.False);
        }

        [Test]
        public void FireEdges_AreIdempotent()
        {
            var state = new PlayerInputState();

            Assert.That(state.PressFire(), Is.True);
            Assert.That(state.PressFire(), Is.False);
            Assert.That(state.ReleaseFire(), Is.True);
            Assert.That(state.ReleaseFire(), Is.False);
        }

        [Test]
        public void LookMode_IsPartOfStateChange()
        {
            var state = new PlayerInputState();
            Vector2 look = new Vector2(100f, 200f);

            Assert.That(state.SetLook(look, isScreenPosition: true), Is.True);
            Assert.That(state.SetLook(look, isScreenPosition: true), Is.False);
            Assert.That(state.SetLook(look, isScreenPosition: false), Is.True);
        }

        [Test]
        public void Reset_ClearsAllInput()
        {
            var state = new PlayerInputState();
            state.SetMove(Vector2.one);
            state.SetLook(Vector2.one, true);
            state.SetSprint(true);
            state.PressFire();

            state.Reset();

            Assert.That(state.Move, Is.EqualTo(Vector2.zero));
            Assert.That(state.Look, Is.EqualTo(Vector2.zero));
            Assert.That(state.Sprint, Is.False);
            Assert.That(state.FireHeld, Is.False);
            Assert.That(state.LookIsScreenPosition, Is.False);
        }
    }
}
