using System.Collections;
using UnityEngine;

public sealed class AutoDispose : MonoBehaviour
{
    [SerializeField, Min(0f)] private float timeToLive = 5f;

    private Coroutine releaseRoutine;

    private void OnEnable()
    {
        releaseRoutine = StartCoroutine(ReleaseAfterLifetime());
    }

    private void OnDisable()
    {
        if (releaseRoutine != null)
        {
            StopCoroutine(releaseRoutine);
            releaseRoutine = null;
        }
    }

    private IEnumerator ReleaseAfterLifetime()
    {
        if (timeToLive > 0f)
        {
            yield return new WaitForSeconds(timeToLive);
        }

        releaseRoutine = null;
        GlobalSystem system = GlobalSystem.Instance;
        if (system != null)
        {
            system.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
