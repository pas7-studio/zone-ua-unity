using System.Collections;
using UnityEngine;

namespace Assets.Script
{
    public class Tools : MonoBehaviour
    {
        public static IEnumerator AttenuateAmmoImpulse(Rigidbody2D ammoRigidbody, float duration)
        {
            float startTime = Time.time;
            while (Time.time - startTime < duration)
            {
                float attenuation = 1.0f - (Time.time - startTime) / duration;
                ammoRigidbody.velocity *= attenuation;
                yield return null;
            }
            Destroy(ammoRigidbody);
        }
    }
}
