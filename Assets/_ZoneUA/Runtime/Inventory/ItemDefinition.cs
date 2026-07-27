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

        public string ItemId => itemId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaximumStack => Mathf.Max(1, maximumStack);
        public GameObject WorldPrefab => worldPrefab;

        private void OnValidate()
        {
            itemId = itemId?.Trim() ?? string.Empty;
            maximumStack = Mathf.Max(1, maximumStack);
        }
    }
}
