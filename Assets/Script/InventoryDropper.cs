using System;
using UnityEngine;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(InventoryComponent))]
public sealed class InventoryDropper : MonoBehaviour
{
    [SerializeField] private ItemCatalog itemCatalog;
    [SerializeField] private Transform dropOrigin;
    [SerializeField] private Transform droppedItemsRoot;
    [SerializeField, Min(0f)] private float forwardOffset = 0.75f;

    private InventoryComponent inventory;

    private void Awake()
    {
        inventory = GetComponent<InventoryComponent>();
        dropOrigin ??= transform;
    }

    public bool Drop(string itemId, int amount = 1)
    {
        Vector3 position = (dropOrigin != null ? dropOrigin.position : transform.position) + transform.right * forwardOffset;
        return DropAt(itemId, amount, position);
    }

    public bool DropAt(string itemId, int amount, Vector3 position)
    {
        inventory ??= GetComponent<InventoryComponent>();
        if (amount <= 0 || itemCatalog == null || !itemCatalog.TryGet(itemId, out ItemDefinition item)) return false;
        if (item.WorldPrefab == null || !inventory.Remove(item.ItemId, amount)) return false;

        GameObject instance = null;
        try
        {
            instance = Instantiate(item.WorldPrefab, position, Quaternion.identity, droppedItemsRoot);
            PersistentIdentity identity = instance.GetComponent<PersistentIdentity>() ?? instance.AddComponent<PersistentIdentity>();
            identity.AssignRuntimeId(Guid.NewGuid().ToString("N"), item.PersistentWorldPrefabId);
            WorldItemPickup pickup = instance.GetComponent<WorldItemPickup>() ?? instance.AddComponent<WorldItemPickup>();
            pickup.Configure(item, amount);
            return true;
        }
        catch (Exception exception)
        {
            inventory.Add(item.ItemId, amount);
            if (instance != null) Destroy(instance);
            Debug.LogException(exception, this);
            return false;
        }
    }

    private void OnValidate() => forwardOffset = Mathf.Max(0f, forwardOffset);
}
