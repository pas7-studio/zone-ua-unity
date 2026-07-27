using UnityEngine;

public class AutoDispose : MonoBehaviour
{
    public float timeToLive = 5.0f; // The number of seconds before the GameObject is destroyed

    void Start()
    {
        Destroy(gameObject, timeToLive);
    }
}