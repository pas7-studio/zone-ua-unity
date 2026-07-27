using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZoneUA.SceneManagement;

namespace ZoneUA.EditorValidation
{
    public static class ZoneUASceneArchitectureTools
    {
        public const string ScenesRoot = "Assets/_ZoneUA/Scenes";
        public const string BootstrapScenePath = ScenesRoot + "/Bootstrap/Bootstrap.unity";
        public const string ProductionScenePath = ScenesRoot + "/Production/MainScene.unity";
        public const string DevelopmentScenePath = ScenesRoot + "/Development/Development.unity";
        public const string TestScenePath = ScenesRoot + "/Tests/Tests.unity";
        public const string CatalogPath = "Assets/_ZoneUA/Settings/SceneCatalog.asset";

        [MenuItem("Zone UA/Scenes/Create Scene Architecture", priority = 1)]
        public static void CreateSceneArchitecture()
        {
            EnsureFolders();
            SceneCatalog catalog = EnsureCatalog();
            EnsureBootstrapScene(catalog);
            EnsureEmptyScene(ProductionScenePath, "ProductionRoot");
            EnsureEmptyScene(DevelopmentScenePath, "DevelopmentRoot");
            EnsureEmptyScene(TestScenePath, "TestRoot");
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
            Debug.Log("Zone UA scene architecture created or updated. Review all generated scenes before committing them.");
        }

        [MenuItem("Zone UA/Scenes/Validate Scene Architecture", priority = 2)]
        public static void ValidateSceneArchitecture()
        {
            IReadOnlyList<string> issues = CollectValidationIssues();
            if (issues.Count == 0)
            {
                Debug.Log("Zone UA scene architecture is valid.");
                return;
            }

            foreach (string issue in issues) Debug.LogError("[Scene Architecture] " + issue);
            EditorUtility.DisplayDialog("Scene architecture validation", $"Found {issues.Count} issue(s). See Console for details.", "OK");
        }

        public static IReadOnlyList<string> CollectValidationIssues()
        {
            var issues = new List<string>();
            SceneCatalog catalog = AssetDatabase.LoadAssetAtPath<SceneCatalog>(CatalogPath);
            if (catalog == null)
            {
                issues.Add($"Missing SceneCatalog at {CatalogPath}.");
            }
            else
            {
                ValidateCatalogScene(catalog.BootstrapScene, issues);
                ValidateCatalogScene(catalog.InitialProductionScene, issues);
                foreach (string scene in catalog.DevelopmentScenes) ValidateCatalogScene(scene, issues);
                foreach (string scene in catalog.TestScenes) ValidateCatalogScene(scene, issues);
            }

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            if (buildScenes.Length == 0)
            {
                issues.Add("Build Settings contains no scenes.");
            }
            else
            {
                string firstEnabled = buildScenes.FirstOrDefault(scene => scene.enabled)?.path ?? string.Empty;
                if (!string.Equals(firstEnabled, BootstrapScenePath, StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add("Bootstrap must be the first enabled Build Settings scene.");
                }
            }

            if (!File.Exists(BootstrapScenePath)) issues.Add($"Missing {BootstrapScenePath}.");
            if (!File.Exists(ProductionScenePath)) issues.Add($"Missing {ProductionScenePath}.");
            return issues;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "_ZoneUA");
            EnsureFolder("Assets/_ZoneUA", "Scenes");
            EnsureFolder(ScenesRoot, "Bootstrap");
            EnsureFolder(ScenesRoot, "Production");
            EnsureFolder(ScenesRoot, "Development");
            EnsureFolder(ScenesRoot, "Tests");
            EnsureFolder("Assets/_ZoneUA", "Settings");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }

        private static SceneCatalog EnsureCatalog()
        {
            SceneCatalog catalog = AssetDatabase.LoadAssetAtPath<SceneCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<SceneCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            SerializedObject serialized = new SerializedObject(catalog);
            serialized.FindProperty("bootstrapScene").stringValue = "Bootstrap";
            serialized.FindProperty("initialProductionScene").stringValue = "Production";
            SetStringList(serialized.FindProperty("developmentScenes"), new[] { "Development" });
            SetStringList(serialized.FindProperty("testScenes"), new[] { "Tests" });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void SetStringList(SerializedProperty property, IReadOnlyList<string> values)
        {
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++) property.GetArrayElementAtIndex(i).stringValue = values[i];
        }

        private static void EnsureBootstrapScene(SceneCatalog catalog)
        {
            if (File.Exists(BootstrapScenePath)) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            try
            {
                GameObject root = new GameObject("ZoneUA_Bootstrap");
                SceneManager.MoveGameObjectToScene(root, scene);
                AddComponentByName(root, "GlobalSystem");
                Component bootstrapper = AddComponentByName(root, "SceneBootstrapper");
                if (bootstrapper != null)
                {
                    SerializedObject serialized = new SerializedObject(bootstrapper);
                    SerializedProperty catalogProperty = serialized.FindProperty("catalog");
                    if (catalogProperty != null) catalogProperty.objectReferenceValue = catalog;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                EditorSceneManager.SaveScene(scene, BootstrapScenePath);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Component AddComponentByName(GameObject target, string typeName)
        {
            Type type = RuntimeTypeResolver.Resolve(typeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogWarning($"Could not resolve component type '{typeName}'. Open the project after scripts compile and run the command again.");
                return null;
            }

            Component existing = target.GetComponent(type);
            return existing != null ? existing : target.AddComponent(type);
        }

        private static void EnsureEmptyScene(string path, string rootName)
        {
            if (File.Exists(path)) return;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            try
            {
                GameObject root = new GameObject(rootName);
                SceneManager.MoveGameObjectToScene(root, scene);
                EditorSceneManager.SaveScene(scene, path);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void ConfigureBuildSettings()
        {
            var desired = new[]
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(ProductionScenePath, true),
                new EditorBuildSettingsScene(DevelopmentScenePath, false),
                new EditorBuildSettingsScene(TestScenePath, false)
            };

            var retained = EditorBuildSettings.scenes
                .Where(scene => desired.All(item => !string.Equals(item.path, scene.path, StringComparison.OrdinalIgnoreCase)) &&
                                !string.Equals(scene.path, "Assets/_ZoneUA/Scenes/Production/Production.unity", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(scene.path, "Assets/Scenes/SampleScene.unity", StringComparison.OrdinalIgnoreCase))
                .ToList();
            retained.InsertRange(0, desired);
            EditorBuildSettings.scenes = retained.ToArray();
        }

        private static void ValidateCatalogScene(string sceneName, ICollection<string> issues)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                issues.Add("SceneCatalog contains an empty scene name.");
                return;
            }

            bool exists = AssetDatabase.FindAssets($"{sceneName} t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Any(path => string.Equals(Path.GetFileNameWithoutExtension(path), sceneName, StringComparison.OrdinalIgnoreCase));
            if (!exists) issues.Add($"SceneCatalog references missing scene '{sceneName}'.");
        }
    }
}
