using System;
using System.Collections.Generic;
using UnityEngine;
using ZoneUA.Inventory;

[DisallowMultipleComponent]
public sealed class InventoryComponent : MonoBehaviour
{
    [SerializeField, Min(0)] private int capacity;
    [SerializeField] private List<InventoryEntry> initialItems = new List<InventoryEntry>();

    private InventoryState state;

    public event Action InventoryChanged;
    public InventoryState State => state ??= CreateState();
    public IReadOnlyList<InventoryEntry> Entries => State.Entries;
    public int Capacity => State.Capacity;
    public int TotalItemCount => State.TotalItemCount;

    private void Awake() => state = CreateState();

    public int GetAmount(string itemId) => State.GetAmount(itemId);
    public bool Has(string itemId, int amount = 1) => State.Has(itemId, amount);

    public bool Add(string itemId, int amount)
    {
        if (!State.Add(itemId, amount)) return false;
        InventoryChanged?.Invoke();
        return true;
    }

    public bool Remove(string itemId, int amount)
    {
        if (!State.Remove(itemId, amount)) return false;
        InventoryChanged?.Invoke();
        return true;
    }

    public bool TryConsume(IEnumerable<InventoryEntry> costs)
    {
        if (!State.TryConsume(costs)) return false;
        InventoryChanged?.Invoke();
        return true;
    }

    public void ReplaceContents(IEnumerable<InventoryEntry> entries)
    {
        State.Replace(entries);
        InventoryChanged?.Invoke();
    }

    private InventoryState CreateState()
    {
        var result = new InventoryState(capacity);
        result.Replace(initialItems);
        return result;
    }

    private void OnValidate() => capacity = Mathf.Max(0, capacity);
}
