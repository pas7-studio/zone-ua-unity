using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZoneUA.EditorValidation
{
    public static class ZoneUAPerformanceTools
    {
        [MenuItem("Zone UA/Performance/Create Stress Harness In Open Scene", priority = 40)]
        public static void CreateStressHarness()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Zone UA Performance", "Open a scene before creating the stress harness.", "OK");
                return;
            }

            Type monitorType = ResolveType("RuntimePerformanceMonitor");
            Type runnerType = ResolveType("PerformanceStressRunner");
            if (monitorType == null || runnerType == null)
            {
                EditorUtility.DisplayDialog("Zone UA Performance", "Runtime performance types are not available. Resolve compilation errors first.", "OK");
                return;
            }

            GameObject existing = GameObject.Find("ZoneUA_PerformanceStressHarness");
            GameObject root = existing != null ? existing : new GameObject("ZoneUA_PerformanceStressHarness");
            Undo.RegisterCreatedObjectUndo(root, "Create performance stress harness");

            if (root.GetComponent(monitorType) == null) Undo.AddComponent(root, monitorType);
            if (root.GetComponent(runnerType) == null) Undo.AddComponent(root, runnerType);

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
            EditorSceneManager.MarkSceneDirty(scene);
        }

        [MenuItem("Zone UA/Performance/Select Stress Harness", priority = 41)]
        public static void SelectStressHarness()
        {
            GameObject root = GameObject.Find("ZoneUA_PerformanceStressHarness");
            if (root == null)
            {
                EditorUtility.DisplayDialog("Zone UA Performance", "No stress harness exists in the open scene.", "OK");
                return;
            }

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        private static Type ResolveType(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try { return assembly.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(type => type.Name == name);
        }
    }
}
