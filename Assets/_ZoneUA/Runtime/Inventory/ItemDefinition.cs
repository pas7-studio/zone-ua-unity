using UnityEngine;

namespace ZoneUA.Inventory
{
    [CreateAssetMenu(menuName = "Zone UA/Inventory/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private Sprite icon;
        [SerializeField, Min(1)] private int maximumStack = 1;
        [SerializeField] private GameObject worldPrefab;
        [SerializeField, Tooltip("Prefab catalog ID used when a dropped item must be recreated from a save.")]
        private string persistentWorldPrefabId = string.Empty;

        public string ItemId => itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaximumStack => Mathf.Max(1, maximumStack);
        public GameObject WorldPrefab => worldPrefab;
        public string PersistentWorldPrefabId => string.IsNullOrWhiteSpace(persistentWorldPrefabId) ? itemId : persistentWorldPrefabId;

        private void OnValidate()
        {
            itemId = itemId?.Trim() ?? string.Empty;
            persistentWorldPrefabId = persistentWorldPrefabId?.Trim() ?? string.Empty;
            maximumStack = Mathf.Max(1, maximumStack);
        }
    }
}
