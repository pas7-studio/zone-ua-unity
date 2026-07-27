using UnityEngine;

public sealed class AutoDispose : MonoBehaviour
{
    [SerializeField, Min(0f)] private float timeToLive = 5f;

    private void Start()
    {
        Destroy(gameObject, timeToLive);
    }
}
