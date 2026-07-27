using System;
using UnityEngine;
using ZoneUA.Persistence;

[DisallowMultipleComponent]
[RequireComponent(typeof(PersistentIdentity))]
[RequireComponent(typeof(Health))]
public sealed class HealthSaveParticipant : MonoBehaviour, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class HealthStateData
    {
        public int currentHealth;
        public int maximumHealth = 1;
    }

    private Health health;

    public string ParticipantKey => "health";
    public int ParticipantVersion => 1;

    private void Awake() => health = GetComponent<Health>();

    public string CaptureState()
    {
        health ??= GetComponent<Health>();
        return JsonUtility.ToJson(new HealthStateData
        {
            currentHealth = health != null ? health.CurrentHealth : 0,
            maximumHealth = health != null ? health.MaximumHealth : 1
        });
    }

    public void RestoreState(string payload, int version)
    {
        health ??= GetComponent<Health>();
        if (health == null || string.IsNullOrWhiteSpace(payload)) return;
        HealthStateData state = JsonUtility.FromJson<HealthStateData>(payload);
        if (state == null) return;
        health.SetMaximumHealth(Mathf.Max(1, state.maximumHealth));
        health.SetHealth(Mathf.Clamp(state.currentHealth, 0, health.MaximumHealth));
    }
}