using System.Collections;
using UnityEngine;

public sealed class AutoDispose : MonoBehaviour
{
    [SerializeField, Min(0f)] private float timeToLive = 5f;

    private Coroutine fallbackRoutine;

    private void OnEnable()
    {
        ScheduleRelease();
    }

    private void OnDisable()
    {
        StopFallbackRoutine();
    }

    public void ScheduleRelease()
    {
        StopFallbackRoutine();

        GlobalSystem system = GlobalSystem.Instance;
        if (system != null)
        {
            system.ReleaseAfter(gameObject, timeToLive);
            return;
        }

        fallbackRoutine = StartCoroutine(FallbackReleaseAfterLifetime());
    }

    private IEnumerator FallbackReleaseAfterLifetime()
    {
        if (timeToLive > 0f)
        {
            yield return new WaitForSeconds(timeToLive);
        }

        fallbackRoutine = null;
        Destroy(gameObject);
    }

    private void StopFallbackRoutine()
    {
        if (fallbackRoutine == null)
        {
            return;
        }

        StopCoroutine(fallbackRoutine);
        fallbackRoutine = null;
    }

    private void OnValidate()
    {
        timeToLive = Mathf.Max(0f, timeToLive);
    }
}
