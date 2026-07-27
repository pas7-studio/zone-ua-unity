using System;

namespace ZoneUA.Combat
{
    public sealed class HealthState
    {
        public HealthState(int maximumHealth, int currentHealth)
        {
            MaximumHealth = Math.Max(1, maximumHealth);
            CurrentHealth = Math.Clamp(currentHealth, 0, MaximumHealth);
        }

        public int CurrentHealth { get; private set; }
        public int MaximumHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        public int SetMaximumHealth(int maximumHealth, bool preserveRatio)
        {
            int previousMaximum = MaximumHealth;
            int previousCurrent = CurrentHealth;
            MaximumHealth = Math.Max(1, maximumHealth);

            if (preserveRatio && previousMaximum > 0)
            {
                float ratio = previousCurrent / (float)previousMaximum;
                CurrentHealth = Math.Clamp((int)Math.Round(MaximumHealth * ratio), 0, MaximumHealth);
            }
            else
            {
                CurrentHealth = Math.Clamp(CurrentHealth, 0, MaximumHealth);
            }

            return CurrentHealth - previousCurrent;
        }

        public int SetHealth(int health)
        {
            int previous = CurrentHealth;
            CurrentHealth = Math.Clamp(health, 0, MaximumHealth);
            return CurrentHealth - previous;
        }

        public int ApplyDamage(int damage)
        {
            if (!IsAlive || damage <= 0)
            {
                return 0;
            }

            int previous = CurrentHealth;
            CurrentHealth = Math.Max(0, CurrentHealth - damage);
            return previous - CurrentHealth;
        }

        public int Heal(int amount)
        {
            if (!IsAlive || amount <= 0)
            {
                return 0;
            }

            int previous = CurrentHealth;
            CurrentHealth = Math.Min(MaximumHealth, CurrentHealth + amount);
            return CurrentHealth - previous;
        }

        public void ResetToMaximum()
        {
            CurrentHealth = MaximumHealth;
        }
    }
}
