using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using ZoneUA.Economy;

namespace ZoneUA.EditorValidation
{
    public static class ZoneUAEconomyContentValidator
    {
        [MenuItem("Zone UA/Validation/Validate Economy Content")]
        public static void ValidateFromMenu()
        {
            IReadOnlyList<string> errors = Validate();
            if (errors.Count == 0)
            {
                Debug.Log("Zone UA economy validation passed.");
                return;
            }
            foreach (string error in errors) Debug.LogError(error);
            Debug.LogError($"Zone UA economy validation failed with {errors.Count} error(s).");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            ValidateResources(errors);
            ValidateRecipes(errors);
            return errors;
        }

        private static void ValidateResources(List<string> errors)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:ResourceNodeDefinition"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ResourceNodeDefinition definition = AssetDatabase.LoadAssetAtPath<ResourceNodeDefinition>(path);
                if (definition == null) continue;
                if (string.IsNullOrWhiteSpace(definition.ResourceId)) errors.Add($"Resource definition has an empty ID: {path}");
                else if (!ids.Add(definition.ResourceId)) errors.Add($"Duplicate resource ID '{definition.ResourceId}': {path}");
                if (definition.YieldedItem == null) errors.Add($"Resource definition has no yielded item: {path}");
            }
        }

        private static void ValidateRecipes(List<string> errors)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:ProductionRecipe"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ProductionRecipe recipe = AssetDatabase.LoadAssetAtPath<ProductionRecipe>(path);
                if (recipe == null) continue;
                if (string.IsNullOrWhiteSpace(recipe.RecipeId)) errors.Add($"Production recipe has an empty ID: {path}");
                else if (!ids.Add(recipe.RecipeId)) errors.Add($"Duplicate production recipe ID '{recipe.RecipeId}': {path}");
                if (recipe.Outputs == null || recipe.Outputs.Count == 0) errors.Add($"Production recipe has no outputs: {path}");
                if (recipe.Inputs.Any(value => value == null || value.item == null || value.amount <= 0)) errors.Add($"Production recipe has an invalid input: {path}");
                if (recipe.Outputs.Any(value => value == null || value.item == null || value.amount <= 0)) errors.Add($"Production recipe has an invalid output: {path}");
            }
        }
    }

    public sealed class ZoneUAEconomyBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 40;

        public void OnPreprocessBuild(BuildReport report)
        {
            IReadOnlyList<string> errors = ZoneUAEconomyContentValidator.Validate();
            if (errors.Count > 0) throw new BuildFailedException(string.Join("\n", errors));
        }
    }
}
