using UnityEngine;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(PersistentIdentity))]
public sealed class WorldItemPickup : MonoBehaviour
{
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int amount = 1;
    [SerializeField] private bool pickupOnTrigger = true;

    private PersistentIdentity identity;
    private bool consumed;

    public ItemDefinition Item => item;
    public int Amount => Mathf.Max(1, amount);

    private void Awake() => identity = GetComponent<PersistentIdentity>();

    public bool TryPickup(InventoryComponent inventory)
    {
        if (consumed || inventory == null || item == null || string.IsNullOrWhiteSpace(item.ItemId)) return false;
        if (!inventory.Add(item.ItemId, Amount)) return false;

        consumed = true;
        identity ??= GetComponent<PersistentIdentity>();
        identity.MarkDestroyed();
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!pickupOnTrigger || other == null) return;
        InventoryComponent inventory = other.GetComponentInParent<InventoryComponent>();
        if (inventory != null) TryPickup(inventory);
    }

    private void OnValidate() => amount = Mathf.Max(1, amount);
}
