using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using ZoneUA.Inventory;

namespace ZoneUA.EditorValidation
{
    public static class ZoneUAInventoryContentValidator
    {
        [MenuItem("Zone UA/Validation/Validate Inventory and Loot", priority = 45)]
        public static void ValidateMenu()
        {
            bool valid = Validate(logResults: true);
            EditorUtility.DisplayDialog("Inventory and loot validation", valid ? "Validation passed." : "Validation failed. See Console.", "OK");
        }

        public static bool Validate(bool logResults)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            ValidateItems(errors, warnings);
            ValidateLoadedObjects(errors, warnings);

            if (logResults)
            {
                foreach (string warning in warnings) Debug.LogWarning("[Inventory Validation] " + warning);
                foreach (string error in errors) Debug.LogError("[Inventory Validation] " + error);
                if (errors.Count == 0) Debug.Log($"Inventory and loot validation passed with {warnings.Count} warning(s).");
            }
            return errors.Count == 0;
        }

        private static void ValidateItems(ICollection<string> errors, ICollection<string> warnings)
        {
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:ItemDefinition"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
                if (item == null) continue;
                if (string.IsNullOrWhiteSpace(item.ItemId)) errors.Add($"ItemDefinition has an empty item ID: {path}");
                else if (seen.TryGetValue(item.ItemId, out string other)) errors.Add($"Duplicate item ID '{item.ItemId}': {other} and {path}");
                else seen[item.ItemId] = path;
                if (item.WorldPrefab == null) warnings.Add($"Item '{item.ItemId}' has no world prefab: {path}");
                if (item.BaseValue == 0) warnings.Add($"Item '{item.ItemId}' has zero base loot value: {path}");
            }
        }

        private static void ValidateLoadedObjects(ICollection<string> errors, ICollection<string> warnings)
        {
            ValidateTypeComposition("InventoryComponent", "InventorySaveParticipant", warnings);
            ValidateTypeComposition("WorldItemPickup", "PersistentIdentity", errors);
            ValidateTypeComposition("LootContainer", "PersistentIdentity", errors);
            ValidateTypeComposition("LootContainer", "InventoryComponent", errors);
            ValidateTypeComposition("LootContainer", "InventorySaveParticipant", errors);
            ValidateTypeComposition("CorpseLootContainer", "LootContainer", errors);
        }

        private static void ValidateTypeComposition(string ownerTypeName, string requiredTypeName, ICollection<string> issues)
        {
            Type ownerType = RuntimeTypeResolver.Resolve(ownerTypeName);
            Type requiredType = RuntimeTypeResolver.Resolve(requiredTypeName);
            if (ownerType == null || requiredType == null) return;
            foreach (UnityEngine.Object item in Resources.FindObjectsOfTypeAll(ownerType))
            {
                if (item is not Component component || EditorUtility.IsPersistent(component)) continue;
                if (component.GetComponent(requiredType) == null)
                    issues.Add($"'{component.name}' contains {ownerTypeName} but is missing {requiredTypeName}.");
            }
        }
    }

    public sealed class ZoneUAInventoryContentBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 90;
        public void OnPreprocessBuild(BuildReport report)
        {
            if (!ZoneUAInventoryContentValidator.Validate(logResults: true))
                throw new BuildFailedException("Inventory or loot content validation failed.");
        }
    }
}
