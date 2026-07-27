using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneUA.Inventory
{
    [CreateAssetMenu(menuName = "Zone UA/Inventory/Item Catalog", fileName = "ItemCatalog")]
    public sealed class ItemCatalog : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();

        public IReadOnlyList<ItemDefinition> Items => items;

        public bool TryGet(string itemId, out ItemDefinition definition)
        {
            definition = null;
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            foreach (ItemDefinition item in items)
            {
                if (item != null && string.Equals(item.ItemId, itemId.Trim(), StringComparison.Ordinal))
                {
                    definition = item;
                    return true;
                }
            }
            return false;
        }
    }
}
