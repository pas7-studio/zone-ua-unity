using System;
using UnityEngine;
using ZoneUA.Economy;
using ZoneUA.Persistence;

[RequireComponent(typeof(PersistentIdentity))]
public sealed class WorkerJobAgent : MonoBehaviour, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class Payload
    {
        public WorkerJobKind kind;
        public string targetObjectId = string.Empty;
    }

    [SerializeField] private InventoryComponent carryInventory;
    [SerializeField, Min(0.01f)] private float workPerSecond = 1f;
    [SerializeField, Min(0.01f)] private float interactionRange = 1f;

    private readonly WorkerJobState state = new WorkerJobState();
    private ResourceNode resourceTarget;
    private InventoryComponent deliveryTarget;

    public string ParticipantKey => "worker-job";
    public int ParticipantVersion => 1;
    public WorkerJobKind CurrentJob => state.kind;

    private void Awake() => carryInventory ??= GetComponent<InventoryComponent>();

    private void Update()
    {
        if (state.kind != WorkerJobKind.Harvest || resourceTarget == null || carryInventory == null) return;
        if (Vector2.Distance(transform.position, resourceTarget.transform.position) > interactionRange) return;

        resourceTarget.ApplyWork(workPerSecond * Time.deltaTime, carryInventory);
        if (resourceTarget.IsDepleted || carryInventory.Capacity > 0 && carryInventory.TotalItemCount >= carryInventory.Capacity)
        {
            if (deliveryTarget != null) DeliverAll(deliveryTarget);
            ClearJob();
        }
    }

    public void AssignHarvest(ResourceNode node, InventoryComponent deposit = null)
    {
        resourceTarget = node;
        deliveryTarget = deposit;
        PersistentIdentity identity = node != null ? node.GetComponent<PersistentIdentity>() : null;
        state.Assign(WorkerJobKind.Harvest, identity != null ? identity.ObjectId : string.Empty);
    }

    public void ClearJob()
    {
        resourceTarget = null;
        deliveryTarget = null;
        state.Clear();
    }

    public int DeliverAll(InventoryComponent destination)
    {
        if (carryInventory == null || destination == null) return 0;
        int delivered = 0;
        var snapshot = new System.Collections.Generic.List<ZoneUA.Inventory.InventoryEntry>(carryInventory.Entries);
        foreach (ZoneUA.Inventory.InventoryEntry entry in snapshot)
        {
            if (!destination.Add(entry.itemId, entry.amount)) continue;
            carryInventory.Remove(entry.itemId, entry.amount);
            delivered += entry.amount;
        }
        return delivered;
    }

    public string CaptureState() => JsonUtility.ToJson(new Payload
    {
        kind = state.kind,
        targetObjectId = state.targetObjectId
    });

    public void RestoreState(string payload, int version)
    {
        Payload restored = JsonUtility.FromJson<Payload>(payload ?? string.Empty);
        if (restored == null) return;
        state.Assign(restored.kind, restored.targetObjectId);
        resourceTarget = null;
        deliveryTarget = null;
    }

    private void OnValidate()
    {
        workPerSecond = Mathf.Max(0.01f, workPerSecond);
        interactionRange = Mathf.Max(0.01f, interactionRange);
    }
}
