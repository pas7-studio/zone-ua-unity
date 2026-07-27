using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZoneUA.EditorValidation
{
    public static class ZoneUAProductionMigrator
    {
        [MenuItem("Zone UA/Migration/Audit Production Composition", priority = 20)]
        public static void AuditProject()
        {
            CompositionMigrationReport report = AuditAssets(FindProductionAssetPaths(), false);
            CompositionMigrationWindow.ShowReport(report);
        }

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
                    "Missing composition components will be added to first-party prefabs under Assets. Prefab GUIDs and existing components are preserved. Continue?",
                    "Migrate",
                    "Cancel"))
            {
                return;
            }

            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsFirstPartyAsset)
                .ToArray();

            CompositionMigrationReport report = AuditAssets(prefabPaths, true);
            AssetDatabase.SaveAssets();
            CompositionMigrationWindow.ShowReport(report);
        }

        [MenuItem("Zone UA/Migration/Audit Open Scenes", priority = 23)]
        public static void AuditOpenScenes()
        {
            CompositionMigrationReport report = new CompositionMigrationReport();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    AuditHierarchy(root, scene.path, false, report);
                }
            }

            CompositionMigrationWindow.ShowReport(report);
        }

        [MenuItem("Zone UA/Migration/Migrate Open Scenes", priority = 24)]
        public static void MigrateOpenScenes()
        {
            CompositionMigrationReport report = new CompositionMigrationReport();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                bool changed = false;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    changed |= AuditHierarchy(root, scene.path, true, report);
                }

                if (changed)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }

            CompositionMigrationWindow.ShowReport(report);
        }

        public static CompositionMigrationReport AuditAssets(IEnumerable<string> assetPaths, bool applyFixes)
        {
            CompositionMigrationReport report = new CompositionMigrationReport();
            foreach (string path in assetPaths.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct())
            {
                if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    AuditPrefab(path, applyFixes, report);
                }
            }

            return report;
        }

        private static void AuditPrefab(string path, bool applyFixes, CompositionMigrationReport report)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
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
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool AuditHierarchy(
            GameObject root,
            string assetPath,
            bool applyFixes,
            CompositionMigrationReport report)
        {
            bool changed = false;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform target in transforms)
            {
                foreach (ProductionCompositionRule rule in ProductionCompositionCatalog.Rules)
                {
                    Type anchorType = RuntimeTypeResolver.Resolve(rule.AnchorTypeName);
                    if (anchorType == null || target.GetComponent(anchorType) == null)
                    {
                        continue;
                    }

                    foreach (string requiredTypeName in rule.RequiredTypeNames)
                    {
                        Type requiredType = RuntimeTypeResolver.Resolve(requiredTypeName);
                        if (requiredType == null)
                        {
                            report.Add(
                                CompositionMigrationStatus.Error,
                                assetPath,
                                GetHierarchyPath(target),
                                $"Required type '{requiredTypeName}' could not be resolved for rule '{rule.Name}'.");
                            continue;
                        }

                        if (target.GetComponent(requiredType) != null)
                        {
                            continue;
                        }

                        if (!typeof(Component).IsAssignableFrom(requiredType))
                        {
                            report.Add(
                                CompositionMigrationStatus.Error,
                                assetPath,
                                GetHierarchyPath(target),
                                $"Required type '{requiredType.FullName}' is not a Component.");
                            continue;
                        }

                        if (!applyFixes)
                        {
                            report.Add(
                                CompositionMigrationStatus.Missing,
                                assetPath,
                                GetHierarchyPath(target),
                                $"Missing {requiredType.Name} required by '{rule.Name}'.");
                            continue;
                        }

                        Undo.AddComponent(target.gameObject, requiredType);
                        changed = true;
                        report.Add(
                            CompositionMigrationStatus.Added,
                            assetPath,
                            GetHierarchyPath(target),
                            $"Added {requiredType.Name} for '{rule.Name}'.");
                    }
                }
            }

            return changed;
        }

        private static IEnumerable<string> FindProductionAssetPaths()
        {
            return AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsFirstPartyAsset);
        }

        private static bool IsFirstPartyAsset(string path)
        {
            return path.StartsWith("Assets/", StringComparison.Ordinal) &&
                   !path.StartsWith("Assets/ThirdParty/", StringComparison.Ordinal) &&
                   !path.Contains("/Samples~/") &&
                   !path.Contains("/Tests/");
        }

        private static string GetHierarchyPath(Transform target)
        {
            var names = new Stack<string>();
            Transform current = target;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }
    }

    internal static class RuntimeTypeResolver
    {
        private static readonly Dictionary<string, Type> Cache = new Dictionary<string, Type>(StringComparer.Ordinal);

        public static Type Resolve(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return null;
            if (Cache.TryGetValue(typeName, out Type cached)) return cached;

            Type resolved = Type.GetType(typeName, false);
            if (resolved == null)
            {
                foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    resolved = assembly.GetType(typeName, false) ??
                               assembly.GetTypes().FirstOrDefault(type => type.Name == typeName);
                    if (resolved != null) break;
                }
            }

            Cache[typeName] = resolved;
            return resolved;
        }
    }
}
