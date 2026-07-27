using UnityEngine;

namespace ZoneUA.Input
{
    public sealed class PlayerInputState
    {
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Sprint { get; private set; }
        public bool LookIsScreenPosition { get; private set; }
        public bool FireHeld { get; private set; }

        public bool SetMove(Vector2 value)
        {
            Vector2 clamped = Vector2.ClampMagnitude(value, 1f);
            if (Move == clamped)
            {
                return false;
            }

            Move = clamped;
            return true;
        }

        public bool SetLook(Vector2 value, bool isScreenPosition)
        {
            if (Look == value && LookIsScreenPosition == isScreenPosition)
            {
                return false;
            }

            Look = value;
            LookIsScreenPosition = isScreenPosition;
            return true;
        }

        public bool SetSprint(bool value)
        {
            if (Sprint == value)
            {
                return false;
            }

            Sprint = value;
            return true;
        }

        public bool PressFire()
        {
            if (FireHeld)
            {
                return false;
            }

            FireHeld = true;
            return true;
        }

        public bool ReleaseFire()
        {
            if (!FireHeld)
            {
                return false;
            }

            FireHeld = false;
            return true;
        }

        public void Reset()
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
            Sprint = false;
            LookIsScreenPosition = false;
            FireHeld = false;
        }
    }
}
