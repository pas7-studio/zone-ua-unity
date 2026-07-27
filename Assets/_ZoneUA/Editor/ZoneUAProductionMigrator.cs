using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZoneUA.EditorValidation
{
    public static class ZoneUAProductionMigrator
    {
        [MenuItem("Zone UA/Migration/Audit Production Composition", priority = 20)]
        public static void AuditProject() =>
            CompositionMigrationWindow.ShowReport(AuditAssets(FindProductionAssetPaths(), false));

        [MenuItem("Zone UA/Migration/Migrate Selected Prefabs", priority = 21)]
        public static void MigrateSelectedPrefabs()
        {
            string[] paths = Selection.objects
                .Select(AssetDatabase.GetAssetPath)
                .Where(path => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToArray();

            CompositionMigrationReport report = AuditAssets(paths, true);
            AssetDatabase.SaveAssets();
            CompositionMigrationWindow.ShowReport(report);
        }

        [MenuItem("Zone UA/Migration/Migrate All Production Prefabs", priority = 22)]
        public static void MigrateAllProductionPrefabs()
        {
            if (!EditorUtility.DisplayDialog(
                    "Migrate production prefabs",
                    "Safe missing composition components will be added to first-party prefabs. Existing GUIDs, components and prefab references are preserved.",
                    "Migrate",
                    "Cancel"))
            {
                return;
            }

            CompositionMigrationReport report = AuditAssets(FindProductionAssetPaths(), true);
            AssetDatabase.SaveAssets();
            CompositionMigrationWindow.ShowReport(report);
        }

        [MenuItem("Zone UA/Migration/Audit Open Scenes", priority = 23)]
        public static void AuditOpenScenes() =>
            CompositionMigrationWindow.ShowReport(ProcessOpenScenes(false));

        [MenuItem("Zone UA/Migration/Migrate Open Scenes", priority = 24)]
        public static void MigrateOpenScenes() =>
            CompositionMigrationWindow.ShowReport(ProcessOpenScenes(true));

        public static CompositionMigrationReport AuditAssets(IEnumerable<string> assetPaths, bool applyFixes)
        {
            var report = new CompositionMigrationReport();
            foreach (string path in assetPaths.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct())
            {
                if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    AuditPrefab(path, applyFixes, report);
                }
            }

            return report;
        }

        private static CompositionMigrationReport ProcessOpenScenes(bool applyFixes)
        {
            var report = new CompositionMigrationReport();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                bool changed = false;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    changed |= AuditHierarchy(root, scene.path, applyFixes, report);
                }

                if (changed && applyFixes)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            return report;
        }

        private static void AuditPrefab(string path, bool applyFixes, CompositionMigrationReport report)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(path);
                if (root == null)
                {
                    report.Add(CompositionMigrationStatus.Error, path, string.Empty, "Prefab contents could not be loaded.");
                    return;
                }

                bool changed = AuditHierarchy(root, path, applyFixes, report);
                if (changed && applyFixes)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
            }
            catch (Exception exception)
            {
                report.Add(CompositionMigrationStatus.Error, path, string.Empty, exception.Message);
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static bool AuditHierarchy(
            GameObject root,
            string assetPath,
            bool applyFixes,
            CompositionMigrationReport report)
        {
            bool changed = false;
            foreach (Transform target in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (ProductionCompositionRule rule in ProductionCompositionCatalog.Rules)
                {
                    Type anchorType = RuntimeTypeResolver.Resolve(rule.AnchorTypeName);
                    if (anchorType == null || target.GetComponent(anchorType) == null) continue;

                    foreach (string requiredTypeName in rule.RequiredTypeNames)
                    {
                        Type requiredType = RuntimeTypeResolver.Resolve(requiredTypeName);
                        string hierarchyPath = GetHierarchyPath(target);

                        if (requiredType == null)
                        {
                            report.Add(
                                CompositionMigrationStatus.Error,
                                assetPath,
                                hierarchyPath,
                                $"Required type '{requiredTypeName}' could not be resolved for '{rule.Name}'.");
                            continue;
                        }

                        if (!typeof(Component).IsAssignableFrom(requiredType))
                        {
                            report.Add(
                                CompositionMigrationStatus.Error,
                                assetPath,
                                hierarchyPath,
                                $"Required type '{requiredType.FullName}' is not a Component.");
                            continue;
                        }

                        if (target.GetComponent(requiredType) != null) continue;

                        if (!applyFixes || !rule.CanAutoAdd)
                        {
                            string suffix = rule.CanAutoAdd ? string.Empty : " Manual reference wiring is required.";
                            report.Add(
                                CompositionMigrationStatus.Missing,
                                assetPath,
                                hierarchyPath,
                                $"Missing {requiredType.Name} required by '{rule.Name}'.{suffix}");
                            continue;
                        }

                        Undo.AddComponent(target.gameObject, requiredType);
                        changed = true;
                        report.Add(
                            CompositionMigrationStatus.Added,
                            assetPath,
                            hierarchyPath,
                            $"Added {requiredType.Name} for '{rule.Name}'.");
                    }
                }
            }

            return changed;
        }

        private static IEnumerable<string> FindProductionAssetPaths() =>
            AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsFirstPartyAsset);

        private static bool IsFirstPartyAsset(string path) =>
            path.StartsWith("Assets/", StringComparison.Ordinal) &&
            !path.StartsWith("Assets/ThirdParty/", StringComparison.Ordinal) &&
            !path.Contains("/Samples~/") &&
            !path.Contains("/Tests/");

        private static string GetHierarchyPath(Transform target)
        {
            var names = new Stack<string>();
            for (Transform current = target; current != null; current = current.parent)
            {
                names.Push(current.name);
            }

            return string.Join("/", names);
        }
    }

    public static class RuntimeTypeResolver
    {
        private static readonly Dictionary<string, Type> Cache =
            new Dictionary<string, Type>(StringComparer.Ordinal);

        public static Type Resolve(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return null;
            if (Cache.TryGetValue(typeName, out Type cached)) return cached;

            Type resolved = Type.GetType(typeName, false);
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (resolved != null) break;
                resolved = assembly.GetType(typeName, false);
                if (resolved != null) break;

                try
                {
                    resolved = assembly.GetTypes().FirstOrDefault(type => type.Name == typeName);
                }
                catch (ReflectionTypeLoadException exception)
                {
                    resolved = exception.Types.FirstOrDefault(type => type != null && type.Name == typeName);
                }
            }

            Cache[typeName] = resolved;
            return resolved;
        }
    }
}
