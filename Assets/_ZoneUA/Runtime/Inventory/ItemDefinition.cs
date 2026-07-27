using UnityEngine;

namespace ZoneUA.Inventory
{
    public enum ItemCategory
    {
        Miscellaneous = 0,
        Weapon = 1,
        Ammunition = 2,
        Medical = 3,
        Armour = 4,
        Consumable = 5,
        Quest = 6,
        Valuable = 7
    }

    [CreateAssetMenu(menuName = "Zone UA/Inventory/Item Definition", fileName = "ItemDefinition")]
    public sealed class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField, TextArea] private string description = string.Empty;
        [SerializeField] private ItemCategory category;
        [SerializeField, Min(0)] private int baseValue;
        [SerializeField, Min(0f)] private float weight;
        [SerializeField] private Sprite icon;
        [SerializeField, Min(1)] private int maximumStack = 1;
        [SerializeField] private GameObject worldPrefab;
        [SerializeField, Tooltip("Prefab catalog ID used when a dropped item must be recreated from a save.")]
        private string persistentWorldPrefabId = string.Empty;

        public string ItemId => itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public ItemCategory Category => category;
        public int BaseValue => Mathf.Max(0, baseValue);
        public float Weight => Mathf.Max(0f, weight);
        public Sprite Icon => icon;
        public int MaximumStack => Mathf.Max(1, maximumStack);
        public GameObject WorldPrefab => worldPrefab;
        public string PersistentWorldPrefabId => string.IsNullOrWhiteSpace(persistentWorldPrefabId) ? itemId : persistentWorldPrefabId;

        private void OnValidate()
        {
            itemId = itemId?.Trim() ?? string.Empty;
            persistentWorldPrefabId = persistentWorldPrefabId?.Trim() ?? string.Empty;
            baseValue = Mathf.Max(0, baseValue);
            weight = Mathf.Max(0f, weight);
            maximumStack = Mathf.Max(1, maximumStack);
        }
    }
}
