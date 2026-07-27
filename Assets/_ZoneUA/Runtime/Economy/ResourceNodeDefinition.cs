using UnityEngine;
using ZoneUA.Inventory;

namespace ZoneUA.Economy
{
    [CreateAssetMenu(menuName = "Zone UA/Economy/Resource Node Definition", fileName = "ResourceNodeDefinition")]
    public sealed class ResourceNodeDefinition : ScriptableObject
    {
        [SerializeField] private string resourceId = string.Empty;
        [SerializeField] private ItemDefinition yieldedItem;
        [SerializeField, Min(1)] private int totalUnits = 10;
        [SerializeField, Min(1)] private int unitsPerHarvest = 1;
        [SerializeField, Min(0.01f)] private float workPerHarvest = 1f;
        [SerializeField, Min(0f)] private float respawnSeconds;

        public string ResourceId => resourceId;
        public ItemDefinition YieldedItem => yieldedItem;
        public int TotalUnits => Mathf.Max(1, totalUnits);
        public int UnitsPerHarvest => Mathf.Max(1, unitsPerHarvest);
        public float WorkPerHarvest => Mathf.Max(0.01f, workPerHarvest);
        public float RespawnSeconds => Mathf.Max(0f, respawnSeconds);

        private void OnValidate()
        {
            resourceId = resourceId?.Trim() ?? string.Empty;
            totalUnits = Mathf.Max(1, totalUnits);
            unitsPerHarvest = Mathf.Clamp(unitsPerHarvest, 1, totalUnits);
            workPerHarvest = Mathf.Max(0.01f, workPerHarvest);
            respawnSeconds = Mathf.Max(0f, respawnSeconds);
        }
    }
}
