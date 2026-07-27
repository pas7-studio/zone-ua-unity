using System;
using System.Collections.Generic;
using UnityEngine;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(PersistentIdentity))]
public sealed class WorldItemPickup : MonoBehaviour, ILootSource, IPersistentSaveParticipant
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
    public string LootSourceId => identity != null ? identity.ObjectId : string.Empty;
    public LootSourceKind Kind => LootSourceKind.WorldItem;
    public LootSearchState SearchState => consumed ? LootSearchState.Empty : LootSearchState.Searched;
    public bool IsAvailable => !consumed && item != null && !string.IsNullOrWhiteSpace(item.ItemId);
    public IReadOnlyList<InventoryEntry> Entries => IsAvailable
        ? new[] { new InventoryEntry(item.ItemId, Amount) }
        : Array.Empty<InventoryEntry>();
    public string ParticipantKey => "world-item";
    public int ParticipantVersion => 1;

    private void Awake() => identity = GetComponent<PersistentIdentity>();

    public void Configure(ItemDefinition definition, int itemAmount)
    {
        item = definition;
        amount = Mathf.Max(1, itemAmount);
        consumed = false;
    }

    public void MarkSearched() { }

    public bool TryTake(InventoryState destination, string itemId, int requestedAmount)
    {
        if (!IsAvailable || destination == null || requestedAmount != Amount || !string.Equals(item.ItemId, itemId, StringComparison.Ordinal)) return false;
        if (!destination.Add(item.ItemId, Amount)) return false;
        Consume();
        return true;
    }

    public bool TryPickup(InventoryComponent inventory)
    {
        if (inventory == null || !TryTake(inventory.State, item != null ? item.ItemId : string.Empty, Amount)) return false;
        inventory.NotifyExternalChange();
        return true;
    }

    public string CaptureState() => JsonUtility.ToJson(new Payload { amount = Amount });

    public void RestoreState(string payload, int version)
    {
        Payload restored = string.IsNullOrWhiteSpace(payload) ? new Payload() : JsonUtility.FromJson<Payload>(payload);
        amount = Mathf.Max(1, restored?.amount ?? 1);
        consumed = false;
    }

    private void Consume()
    {
        consumed = true;
        identity ??= GetComponent<PersistentIdentity>();
        identity.MarkDestroyed();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!pickupOnTrigger || other == null) return;
        InventoryComponent inventory = other.GetComponentInParent<InventoryComponent>();
        if (inventory != null) TryPickup(inventory);
    }

    private void OnValidate() => amount = Mathf.Max(1, amount);
}
