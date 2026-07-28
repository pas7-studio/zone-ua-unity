using System.Collections;
using UnityEngine;

namespace Assets.Script
{
    public static class Tools
    {
        public static IEnumerator AttenuateVelocity(Rigidbody2D body, float duration)
        {
            if (body == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                yield break;
            }

            Vector2 initialVelocity = body.linearVelocity;
            float initialAngularVelocity = body.angularVelocity;
            float elapsed = 0f;

            while (body != null && elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float attenuation = 1f - progress;

                body.linearVelocity = initialVelocity * attenuation;
                body.angularVelocity = initialAngularVelocity * attenuation;
                yield return null;
            }

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }

        // Backwards-compatible API for existing callers.
        public static IEnumerator AttenuateAmmoImpulse(Rigidbody2D body, float duration)
        {
            return AttenuateVelocity(body, duration);
        }
    }
}
