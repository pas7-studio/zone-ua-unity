using System;
using System.Collections.Generic;
using UnityEngine;
using ZoneUA.Inventory;

namespace ZoneUA.Economy
{
    [Serializable]
    public sealed class ProductionAmount
    {
        public ItemDefinition item;
        [Min(1)] public int amount = 1;
    }

    [CreateAssetMenu(menuName = "Zone UA/Economy/Production Recipe", fileName = "ProductionRecipe")]
    public sealed class ProductionRecipe : ScriptableObject
    {
        [SerializeField] private string recipeId = string.Empty;
        [SerializeField, Min(0.01f)] private float durationSeconds = 1f;
        [SerializeField] private List<ProductionAmount> inputs = new List<ProductionAmount>();
        [SerializeField] private List<ProductionAmount> outputs = new List<ProductionAmount>();

        public string RecipeId => recipeId;
        public float DurationSeconds => Mathf.Max(0.01f, durationSeconds);
        public IReadOnlyList<ProductionAmount> Inputs => inputs;
        public IReadOnlyList<ProductionAmount> Outputs => outputs;

        public List<InventoryEntry> BuildInputs() => BuildEntries(inputs);
        public List<InventoryEntry> BuildOutputs() => BuildEntries(outputs);

        private static List<InventoryEntry> BuildEntries(IEnumerable<ProductionAmount> amounts)
        {
            var result = new List<InventoryEntry>();
            if (amounts == null) return result;
            foreach (ProductionAmount entry in amounts)
            {
                if (entry?.item == null || string.IsNullOrWhiteSpace(entry.item.ItemId) || entry.amount <= 0) continue;
                result.Add(new InventoryEntry(entry.item.ItemId, entry.amount));
            }
            return result;
        }

        private void OnValidate()
        {
            recipeId = recipeId?.Trim() ?? string.Empty;
            durationSeconds = Mathf.Max(0.01f, durationSeconds);
        }
    }
}
