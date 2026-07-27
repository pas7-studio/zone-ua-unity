using System;

namespace ZoneUA.AI
{
    public enum NpcState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee,
        Dead
    }

    public sealed class NpcBrainState
    {
        public NpcState Current { get; private set; } = NpcState.Idle;
        public float LastTargetSeenTime { get; private set; } = float.NegativeInfinity;
        public bool HasTarget { get; private set; }

        public event Action<NpcState, NpcState> Changed;

        public void SetTargetVisible(bool visible, float currentTime)
        {
            HasTarget = visible;
            if (visible)
            {
                LastTargetSeenTime = currentTime;
            }
        }

        public NpcState Evaluate(
            bool isAlive,
            float healthFraction,
            float fleeThreshold,
            bool hasPatrol,
            float targetDistance,
            float attackDistance,
            float loseTargetDelay,
            float currentTime)
        {
            NpcState next;
            if (!isAlive)
            {
                next = NpcState.Dead;
            }
            else if (healthFraction <= Math.Clamp(fleeThreshold, 0f, 1f) && HasTarget)
            {
                next = NpcState.Flee;
            }
            else if (HasTarget && targetDistance <= Math.Max(0f, attackDistance))
            {
                next = NpcState.Attack;
            }
            else if (HasTarget || currentTime - LastTargetSeenTime <= Math.Max(0f, loseTargetDelay))
            {
                next = NpcState.Chase;
            }
            else if (hasPatrol)
            {
                next = NpcState.Patrol;
            }
            else
            {
                next = NpcState.Idle;
            }

            Transition(next);
            return Current;
        }

        public void ClearTarget()
        {
            HasTarget = false;
            LastTargetSeenTime = float.NegativeInfinity;
        }

        public bool Transition(NpcState next)
        {
            if (Current == NpcState.Dead || Current == next)
            {
                return false;
            }

            NpcState previous = Current;
            Current = next;
            Changed?.Invoke(previous, next);
            return true;
        }

        public void Reset()
        {
            Current = NpcState.Idle;
            ClearTarget();
        }
    }
}
