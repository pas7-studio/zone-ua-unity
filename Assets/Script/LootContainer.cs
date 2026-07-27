using System;
using System.Collections.Generic;
using UnityEngine;
using ZoneUA.Inventory;
using ZoneUA.Persistence;

[RequireComponent(typeof(PersistentIdentity), typeof(InventoryComponent), typeof(InventorySaveParticipant))]
public sealed class LootContainer : MonoBehaviour, ILootSource, IPersistentSaveParticipant
{
    [Serializable]
    private sealed class Payload
    {
        public LootSearchState searchState;
    }

    [SerializeField] private LootSourceKind kind = LootSourceKind.Container;
    [SerializeField, Min(0f)] private float searchDurationSeconds = 1f;
    [SerializeField] private bool startsSearched;

    private PersistentIdentity identity;
    private InventoryComponent inventory;
    private LootSearchState searchState;

    public event Action<LootSearchState> SearchStateChanged;
    public event Action ContentsChanged;

    public string LootSourceId => identity != null ? identity.ObjectId : string.Empty;
    public LootSourceKind Kind => kind;
    public LootSearchState SearchState => searchState;
    public bool IsAvailable => inventory != null && inventory.TotalItemCount > 0;
    public IReadOnlyList<InventoryEntry> Entries => inventory != null ? inventory.Entries : Array.Empty<InventoryEntry>();
    public float SearchDurationSeconds => Mathf.Max(0f, searchDurationSeconds);
    public string ParticipantKey => "loot-source";
    public int ParticipantVersion => 1;

    private void Awake()
    {
        identity = GetComponent<PersistentIdentity>();
        inventory = GetComponent<InventoryComponent>();
        searchState = startsSearched ? ResolveVisibleState() : LootSearchState.Unsearched;
        inventory.InventoryChanged += HandleContentsChanged;
    }

    private void OnDestroy()
    {
        if (inventory != null) inventory.InventoryChanged -= HandleContentsChanged;
    }

    public void MarkSearched()
    {
        LootSearchState next = ResolveVisibleState();
        if (searchState == next) return;
        searchState = next;
        SearchStateChanged?.Invoke(searchState);
    }

    public bool TryTake(InventoryState destination, string itemId, int amount)
    {
        if (searchState == LootSearchState.Unsearched || inventory == null || destination == null) return false;
        bool transferred = inventory.State.TryTransferTo(destination, itemId, amount);
        if (transferred) HandleContentsChanged();
        return transferred;
    }

    public bool TryTake(InventoryComponent destination, string itemId, int amount)
    {
        if (destination == null) return false;
        bool transferred = TryTake(destination.State, itemId, amount);
        if (transferred) destination.NotifyExternalChange();
        return transferred;
    }

    public string CaptureState() => JsonUtility.ToJson(new Payload { searchState = searchState });

    public void RestoreState(string payload, int version)
    {
        Payload restored = string.IsNullOrWhiteSpace(payload) ? null : JsonUtility.FromJson<Payload>(payload);
        searchState = restored != null ? restored.searchState : startsSearched ? ResolveVisibleState() : LootSearchState.Unsearched;
        if (!IsAvailable) searchState = LootSearchState.Empty;
        SearchStateChanged?.Invoke(searchState);
    }

    private void HandleContentsChanged()
    {
        if (!IsAvailable && searchState != LootSearchState.Empty)
        {
            searchState = LootSearchState.Empty;
            SearchStateChanged?.Invoke(searchState);
        }
        ContentsChanged?.Invoke();
    }

    private LootSearchState ResolveVisibleState() => IsAvailable ? LootSearchState.Searched : LootSearchState.Empty;

    private void OnValidate() => searchDurationSeconds = Mathf.Max(0f, searchDurationSeconds);
}
