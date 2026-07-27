using System;
using UnityEngine;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(PersistentIdentity))]
public sealed class ConstructionSite : MonoBehaviour, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class Payload
    {
        public string constructionId = string.Empty;
        public float requiredWork;
        public float appliedWork;
        public bool resourcesCommitted;
    }

    [SerializeField] private ConstructionDefinition definition;
    [SerializeField] private GameObject incompleteRoot;
    [SerializeField] private GameObject completedRoot;

    private ConstructionState state;

    public event Action<float> ProgressChanged;
    public event Action Completed;

    public ConstructionDefinition Definition => definition;
    public bool IsComplete => State.IsComplete;
    public float Progress01 => State.Progress01;
    public string ParticipantKey => "construction";
    public int ParticipantVersion => 1;
    private ConstructionState State => state ??= new ConstructionState(definition != null ? definition.RequiredWork : 1f);

    private void Awake()
    {
        state = new ConstructionState(definition != null ? definition.RequiredWork : 1f);
        RefreshVisuals();
    }

    public bool CommitResources(InventoryComponent inventory)
    {
        if (definition == null || inventory == null || State.ResourcesCommitted) return false;
        if (!inventory.TryConsume(definition.BuildCostEntries())) return false;
        State.CommitResources();
        ProgressChanged?.Invoke(State.Progress01);
        return true;
    }

    public float ApplyWork(float amount)
    {
        bool wasComplete = State.IsComplete;
        float applied = State.ApplyWork(amount);
        if (applied <= 0f) return 0f;
        ProgressChanged?.Invoke(State.Progress01);
        if (!wasComplete && State.IsComplete)
        {
            RefreshVisuals();
            Completed?.Invoke();
        }
        return applied;
    }

    public string CaptureState()
    {
        var payload = new Payload
        {
            constructionId = definition != null ? definition.ConstructionId : string.Empty,
            requiredWork = State.RequiredWork,
            appliedWork = State.AppliedWork,
            resourcesCommitted = State.ResourcesCommitted
        };
        return JsonUtility.ToJson(payload);
    }

    public void RestoreState(string payload, int version)
    {
        Payload restored = string.IsNullOrWhiteSpace(payload) ? new Payload() : JsonUtility.FromJson<Payload>(payload);
        float required = restored != null && restored.requiredWork > 0f
            ? restored.requiredWork
            : definition != null ? definition.RequiredWork : 1f;
        State.Restore(required, restored?.appliedWork ?? 0f, restored?.resourcesCommitted ?? false);
        RefreshVisuals();
        ProgressChanged?.Invoke(State.Progress01);
    }

    private void RefreshVisuals()
    {
        if (incompleteRoot != null) incompleteRoot.SetActive(!State.IsComplete);
        if (completedRoot != null) completedRoot.SetActive(State.IsComplete);
    }
}
