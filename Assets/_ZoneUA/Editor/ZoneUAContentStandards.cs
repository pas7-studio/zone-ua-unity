using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace ZoneUA.EditorValidation
{
    [Serializable]
    public sealed class ContentStandardsReport
    {
        public readonly List<ValidationIssue> issues = new List<ValidationIssue>();
        public int ErrorCount => issues.Count(issue => issue.severity == ValidationSeverity.Error);
        public int WarningCount => issues.Count(issue => issue.severity == ValidationSeverity.Warning);
        public bool IsValid => ErrorCount == 0;

        public void Add(ValidationSeverity severity, string code, string message, string path = "")
        {
            issues.Add(new ValidationIssue(severity, code, message, path));
        }
    }

    public static class ZoneUAContentStandards
    {
        private static readonly string[] RequiredSortingLayers =
        {
            "BackGroundTiles", "GeneratedAndPlayer", "TargetMouse", "TopLayer", "UI"
        };

        private static readonly string[] RequiredPhysicsLayers =
        {
            "Player", "NPC", "Projectile", "World", "Interactable"
        };

        private const float RootScaleTolerance = 0.0001f;
        private const float SpriteZTolerance = 0.001f;
        private const int RecommendedPixelsPerUnit = 16;

        [MenuItem("Zone UA/Validation/Validate Rendering and Physics", priority = 10)]
        public static void ValidateMenu()
        {
            ContentStandardsReport report = ValidateProject();
            foreach (ValidationIssue issue in report.issues)
            {
                string text = $"[{issue.code}] {issue.message} {issue.assetPath}";
                if (issue.severity == ValidationSeverity.Error) Debug.LogError(text);
                else if (issue.severity == ValidationSeverity.Warning) Debug.LogWarning(text);
                else Debug.Log(text);
            }

            EditorUtility.DisplayDialog(
                "Zone UA content standards",
                $"Errors: {report.ErrorCount}\nWarnings: {report.WarningCount}\n\nSee Console for details.",
                "OK");
        }

        public static ContentStandardsReport ValidateProject()
        {
            ContentStandardsReport report = new ContentStandardsReport();
            ValidateProjectLayers(report);
            ValidatePrefabs(report);
            ValidateSpriteImports(report);
            ValidateDuplicateMaterials(report);
            return report;
        }

        private static void ValidateProjectLayers(ContentStandardsReport report)
        {
            foreach (string layer in RequiredSortingLayers)
            {
                if (!SortingLayer.layers.Any(value => value.name == layer))
                {
                    report.Add(ValidationSeverity.Error, "SORTING_LAYER_MISSING", $"Required sorting layer '{layer}' is missing.", "ProjectSettings/TagManager.asset");
                }
            }

            foreach (string layer in RequiredPhysicsLayers)
            {
                if (LayerMask.NameToLayer(layer) < 0)
                {
                    report.Add(ValidationSeverity.Warning, "PHYSICS_LAYER_MISSING", $"Recommended physics layer '{layer}' is missing.", "ProjectSettings/TagManager.asset");
                }
            }
        }

        private static void ValidatePrefabs(ContentStandardsReport report)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsFirstParty(path)) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    ValidateRootTransform(root.transform, path, report);
                    ValidateHierarchy(root, path, report);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void ValidateRootTransform(Transform root, string path, ContentStandardsReport report)
        {
            if ((root.localScale - Vector3.one).sqrMagnitude > RootScaleTolerance)
            {
                report.Add(ValidationSeverity.Warning, "PREFAB_ROOT_SCALE", "Prefab root scale should normally be (1,1,1).", path);
            }

            if (root.localPosition.sqrMagnitude > RootScaleTolerance || Quaternion.Angle(root.localRotation, Quaternion.identity) > 0.01f)
            {
                report.Add(ValidationSeverity.Warning, "PREFAB_ROOT_TRANSFORM", "Prefab root should normally use zero position and identity rotation.", path);
            }
        }

        private static void ValidateHierarchy(GameObject root, string path, ContentStandardsReport report)
        {
            SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length > 1 && root.GetComponentInChildren<SortingGroup>(true) == null)
            {
                report.Add(ValidationSeverity.Warning, "SORTING_GROUP_RECOMMENDED", "Prefab contains multiple SpriteRenderers but no SortingGroup.", path);
            }

            foreach (SpriteRenderer renderer in renderers)
            {
                if (Mathf.Abs(renderer.transform.localPosition.z) > SpriteZTolerance)
                {
                    report.Add(ValidationSeverity.Warning, "ARBITRARY_SPRITE_Z", $"SpriteRenderer '{renderer.name}' uses a non-zero local Z offset; prefer sorting layers/order.", path);
                }

                if (string.IsNullOrWhiteSpace(renderer.sortingLayerName))
                {
                    report.Add(ValidationSeverity.Error, "SORTING_LAYER_INVALID", $"SpriteRenderer '{renderer.name}' has no valid sorting layer.", path);
                }
            }

            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                Vector3 scale = transform.localScale;
                if (scale.x < 0f || scale.y < 0f || scale.z < 0f)
                {
                    report.Add(ValidationSeverity.Warning, "NEGATIVE_SCALE", $"'{GetHierarchyPath(transform)}' uses negative scale; prefer SpriteRenderer.flipX/flipY where possible.", path);
                }
            }

            foreach (BoxCollider2D collider in root.GetComponentsInChildren<BoxCollider2D>(true))
            {
                if (collider.size.x <= 0f || collider.size.y <= 0f)
                {
                    report.Add(ValidationSeverity.Error, "COLLIDER_ZERO_SIZE", $"BoxCollider2D '{GetHierarchyPath(collider.transform)}' has zero size.", path);
                }
            }

            foreach (CircleCollider2D collider in root.GetComponentsInChildren<CircleCollider2D>(true))
            {
                if (collider.radius <= 0f)
                {
                    report.Add(ValidationSeverity.Error, "COLLIDER_ZERO_RADIUS", $"CircleCollider2D '{GetHierarchyPath(collider.transform)}' has zero radius.", path);
                }
            }
        }

        private static void ValidateSpriteImports(ContentStandardsReport report)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsFirstParty(path)) continue;
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer) || importer.textureType != TextureImporterType.Sprite) continue;

                if (!Mathf.Approximately(importer.spritePixelsPerUnit, RecommendedPixelsPerUnit))
                {
                    report.Add(ValidationSeverity.Warning, "SPRITE_PPU", $"Sprite uses {importer.spritePixelsPerUnit} PPU; project recommendation is {RecommendedPixelsPerUnit}.", path);
                }

                if (importer.filterMode != FilterMode.Point)
                {
                    report.Add(ValidationSeverity.Warning, "SPRITE_FILTER", "Pixel-art sprite should normally use Point filtering.", path);
                }

                if (importer.mipmapEnabled)
                {
                    report.Add(ValidationSeverity.Warning, "SPRITE_MIPMAPS", "2D pixel-art sprite has mipmaps enabled.", path);
                }
            }
        }

        private static void ValidateDuplicateMaterials(ContentStandardsReport report)
        {
            Dictionary<string, string> firstPathBySignature = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!IsFirstParty(path)) continue;
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) continue;

                string signature = material.shader != null ? material.shader.name + "|" + EditorJsonUtility.ToJson(material) : EditorJsonUtility.ToJson(material);
                if (firstPathBySignature.TryGetValue(signature, out string existing))
                {
                    report.Add(ValidationSeverity.Warning, "DUPLICATE_MATERIAL", $"Material duplicates '{existing}'. Reuse one shared material where possible.", path);
                }
                else
                {
                    firstPathBySignature.Add(signature, path);
                }
            }
        }

        private static bool IsFirstParty(string path)
        {
            return path.StartsWith("Assets/", StringComparison.Ordinal) &&
                   !path.StartsWith("Assets/ThirdParty/", StringComparison.Ordinal) &&
                   !path.Contains("/Tests/") &&
                   !path.Contains("/Samples~/");
        }

        private static string GetHierarchyPath(Transform target)
        {
            Stack<string> names = new Stack<string>();
            while (target != null)
            {
                names.Push(target.name);
                target = target.parent;
            }
            return string.Join("/", names);
        }
    }

    public sealed class ZoneUAContentStandardsBuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -900;

        public void OnPreprocessBuild(BuildReport report)
        {
            ContentStandardsReport validation = ZoneUAContentStandards.ValidateProject();
            if (!validation.IsValid)
            {
                throw new BuildFailedException($"Zone UA content standards failed with {validation.ErrorCount} error(s).");
            }
        }
    }
}
