using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneUA.Inventory
{
    [Serializable]
    public sealed class ConstructionCost
    {
        public ItemDefinition item;
        [Min(1)] public int amount = 1;
    }

    [CreateAssetMenu(menuName = "Zone UA/Construction/Definition", fileName = "ConstructionDefinition")]
    public sealed class ConstructionDefinition : ScriptableObject
    {
        [SerializeField] private string constructionId = string.Empty;
        [SerializeField] private GameObject completedPrefab;
        [SerializeField, Min(0.01f)] private float requiredWork = 1f;
        [SerializeField] private List<ConstructionCost> costs = new List<ConstructionCost>();

        public string ConstructionId => constructionId;
        public GameObject CompletedPrefab => completedPrefab;
        public float RequiredWork => Mathf.Max(0.01f, requiredWork);
        public IReadOnlyList<ConstructionCost> Costs => costs;

        public List<InventoryEntry> BuildCostEntries()
        {
            var result = new List<InventoryEntry>();
            foreach (ConstructionCost cost in costs)
            {
                if (cost?.item == null || string.IsNullOrWhiteSpace(cost.item.ItemId) || cost.amount <= 0) continue;
                result.Add(new InventoryEntry(cost.item.ItemId, cost.amount));
            }
            return result;
        }

        private void OnValidate()
        {
            constructionId = constructionId?.Trim() ?? string.Empty;
            requiredWork = Mathf.Max(0.01f, requiredWork);
        }
    }
}
