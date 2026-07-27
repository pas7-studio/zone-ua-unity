using System;
using UnityEngine;
using ZoneUA.Economy;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(PersistentIdentity))]
public sealed class ResourceNode : MonoBehaviour, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class Payload
    {
        public string resourceId = string.Empty;
        public int remainingUnits;
        public float accumulatedWork;
        public float respawnRemaining;
    }

    [SerializeField] private ResourceNodeDefinition definition;
    [SerializeField] private GameObject availableRoot;
    [SerializeField] private GameObject depletedRoot;

    private HarvestState state;

    public string ParticipantKey => "resource-node";
    public int ParticipantVersion => 1;
    public ResourceNodeDefinition Definition => definition;
    public int RemainingUnits => state?.remainingUnits ?? 0;
    public bool IsDepleted => state == null || state.IsDepleted;

    private void Awake()
    {
        EnsureState();
        RefreshPresentation();
    }

    private void Update()
    {
        if (state == null || !state.IsDepleted || definition == null || definition.RespawnSeconds <= 0f) return;
        state.respawnRemaining = Mathf.Max(0f, state.respawnRemaining - Time.deltaTime);
        if (state.respawnRemaining <= 0f)
        {
            state.Restore(definition.TotalUnits, 0f, 0f);
            RefreshPresentation();
        }
    }

    public int ApplyWork(float work, InventoryComponent destination)
    {
        EnsureState();
        if (definition == null || destination == null || definition.YieldedItem == null) return 0;

        int harvested = state.ApplyWork(work, definition.WorkPerHarvest, definition.UnitsPerHarvest);
        if (harvested <= 0) return 0;

        if (!destination.Add(definition.YieldedItem.ItemId, harvested))
        {
            state.remainingUnits += harvested;
            return 0;
        }

        if (state.IsDepleted && definition.RespawnSeconds > 0f) state.respawnRemaining = definition.RespawnSeconds;
        RefreshPresentation();
        return harvested;
    }

    public string CaptureState()
    {
        EnsureState();
        return JsonUtility.ToJson(new Payload
        {
            resourceId = definition != null ? definition.ResourceId : string.Empty,
            remainingUnits = state.remainingUnits,
            accumulatedWork = state.accumulatedWork,
            respawnRemaining = state.respawnRemaining
        });
    }

    public void RestoreState(string payload, int version)
    {
        EnsureState();
        Payload restored = JsonUtility.FromJson<Payload>(payload ?? string.Empty);
        if (restored == null) return;
        state.Restore(restored.remainingUnits, restored.accumulatedWork, restored.respawnRemaining);
        RefreshPresentation();
    }

    private void EnsureState()
    {
        if (state == null) state = new HarvestState(definition != null ? definition.TotalUnits : 0);
    }

    private void RefreshPresentation()
    {
        if (availableRoot != null) availableRoot.SetActive(!IsDepleted);
        if (depletedRoot != null) depletedRoot.SetActive(IsDepleted);
    }
}
