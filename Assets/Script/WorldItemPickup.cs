using System;
using UnityEngine;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(PersistentIdentity))]
public sealed class WorldItemPickup : MonoBehaviour, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class Payload
    {
        public int amount = 1;
    }

    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int amount = 1;
    [SerializeField] private bool pickupOnTrigger = true;

    private PersistentIdentity identity;
    private bool consumed;

    public ItemDefinition Item => item;
    public int Amount => Mathf.Max(1, amount);
    public string ParticipantKey => "world-item";
    public int ParticipantVersion => 1;

    private void Awake() => identity = GetComponent<PersistentIdentity>();

    public void Configure(ItemDefinition definition, int itemAmount)
    {
        item = definition;
        amount = Mathf.Max(1, itemAmount);
        consumed = false;
    }

    public bool TryPickup(InventoryComponent inventory)
    {
        if (consumed || inventory == null || item == null || string.IsNullOrWhiteSpace(item.ItemId)) return false;
        if (!inventory.Add(item.ItemId, Amount)) return false;

        consumed = true;
        identity ??= GetComponent<PersistentIdentity>();
        identity.MarkDestroyed();
        return true;
    }

    public string CaptureState() => JsonUtility.ToJson(new Payload { amount = Amount });

    public void RestoreState(string payload, int version)
    {
        Payload restored = string.IsNullOrWhiteSpace(payload) ? new Payload() : JsonUtility.FromJson<Payload>(payload);
        amount = Mathf.Max(1, restored?.amount ?? 1);
        consumed = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!pickupOnTrigger || other == null) return;
        InventoryComponent inventory = other.GetComponentInParent<InventoryComponent>();
        if (inventory != null) TryPickup(inventory);
    }

    private void OnValidate() => amount = Mathf.Max(1, amount);
}
