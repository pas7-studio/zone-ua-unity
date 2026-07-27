using System;
using System.Collections.Generic;
using UnityEngine;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(InventoryComponent))]
public sealed class InventorySaveParticipant : MonoBehaviour, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class Payload
    {
        public List<InventoryEntry> entries = new List<InventoryEntry>();
    }

    private InventoryComponent inventory;

    public string ParticipantKey => "inventory";
    public int ParticipantVersion => 1;

    private void Awake() => inventory = GetComponent<InventoryComponent>();

    public string CaptureState()
    {
        inventory ??= GetComponent<InventoryComponent>();
        return JsonUtility.ToJson(new Payload { entries = new List<InventoryEntry>(inventory.Entries) });
    }

    public void RestoreState(string payload, int version)
    {
        inventory ??= GetComponent<InventoryComponent>();
        Payload state = string.IsNullOrWhiteSpace(payload) ? new Payload() : JsonUtility.FromJson<Payload>(payload);
        inventory.ReplaceContents(state?.entries ?? new List<InventoryEntry>());
    }
}
