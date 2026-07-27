using UnityEngine;

namespace ZoneUA.Combat
{
    [DisallowMultipleComponent]
    public sealed class WeaponRecoil : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float fallbackIncreasePerShot = 0.1f;
        [SerializeField, Min(0f)] private float fallbackMaximum = 2f;
        [SerializeField, Min(0f)] private float fallbackRecoveryPerSecond = 1f;
        [SerializeField, Min(0f)] private float verticalPositiveScale = 1.5f;
        [SerializeField, Min(0f)] private float verticalNegativeScale = 0.5f;

        private float currentAmount;

        public float CurrentAmount => currentAmount;

        public void Tick(WeaponDefinition definition, float deltaTime)
        {
            float recovery = definition != null
                ? definition.SpreadRecoveryPerSecond
                : fallbackRecoveryPerSecond;

            currentAmount = Mathf.MoveTowards(currentAmount, 0f, Mathf.Max(0f, recovery) * deltaTime);
        }

        public void RegisterShot(WeaponDefinition definition)
        {
            float increase = definition != null
                ? definition.SpreadPerShot
                : fallbackIncreasePerShot;
            float maximum = definition != null
                ? definition.MaximumSpread
                : fallbackMaximum;

            currentAmount = Mathf.Min(Mathf.Max(0f, maximum), currentAmount + Mathf.Max(0f, increase));
        }

        public float ApplyToAngle(float angle, bool facingLeft)
        {
            float minimum = -currentAmount * (facingLeft ? verticalPositiveScale : verticalNegativeScale);
            float maximum = currentAmount * (facingLeft ? verticalNegativeScale : verticalPositiveScale);
            return angle + Random.Range(minimum, maximum);
        }

        public void ResetState()
        {
            currentAmount = 0f;
        }

        private void OnValidate()
        {
            fallbackIncreasePerShot = Mathf.Max(0f, fallbackIncreasePerShot);
            fallbackMaximum = Mathf.Max(0f, fallbackMaximum);
            fallbackRecoveryPerSecond = Mathf.Max(0f, fallbackRecoveryPerSecond);
            verticalPositiveScale = Mathf.Max(0f, verticalPositiveScale);
            verticalNegativeScale = Mathf.Max(0f, verticalNegativeScale);
        }
    }
}
