using System.Collections.Generic;
using UnityEngine;
using ZoneUA.Inventory;

[RequireComponent(typeof(LootContainer), typeof(InventoryComponent))]
public sealed class CorpseLootContainer : MonoBehaviour
{
    [SerializeField] private bool clearSourceInventory = true;

    private InventoryComponent corpseInventory;
    private LootContainer lootContainer;

    public bool IsInitialised { get; private set; }

    private void Awake()
    {
        corpseInventory = GetComponent<InventoryComponent>();
        lootContainer = GetComponent<LootContainer>();
    }

    public bool InitialiseFrom(InventoryComponent sourceInventory)
    {
        if (IsInitialised || sourceInventory == null || sourceInventory == corpseInventory) return false;

        List<InventoryEntry> snapshot = new List<InventoryEntry>(sourceInventory.Entries);
        corpseInventory.ReplaceContents(snapshot);
        if (clearSourceInventory) sourceInventory.ReplaceContents(System.Array.Empty<InventoryEntry>());

        lootContainer.MarkSearched();
        IsInitialised = true;
        return true;
    }
}
