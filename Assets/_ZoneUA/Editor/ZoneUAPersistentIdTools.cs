using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZoneUA.Persistence;

namespace ZoneUA.EditorValidation
{
    public static class ZoneUAPersistentIdTools
    {
        [MenuItem("Zone UA/Persistence/Assign Missing Stable IDs", priority = 10)]
        public static void AssignMissingStableIds()
        {
            int assigned = 0;
            foreach (PersistentIdentity identity in FindSceneIdentities())
            {
                if (identity.HasValidId || identity.RuntimeSpawned) continue;
                Undo.RecordObject(identity, "Assign persistent object ID");
                SerializedObject serialized = new SerializedObject(identity);
                serialized.FindProperty("objectId").stringValue = Guid.NewGuid().ToString("N");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(identity);
                if (identity.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(identity.gameObject.scene);
                assigned++;
            }
            Debug.Log($"Assigned {assigned} persistent object ID(s). Save the modified scenes and prefabs after review.");
        }

        [MenuItem("Zone UA/Persistence/Validate Stable IDs", priority = 11)]
        public static void ValidateStableIds()
        {
            List<string> issues = CollectIssues();
            if (issues.Count == 0)
            {
                Debug.Log("Persistent stable ID validation passed.");
                return;
            }
            foreach (string issue in issues) Debug.LogError("[Persistent ID] " + issue);
            EditorUtility.DisplayDialog("Persistent ID validation", $"Found {issues.Count} issue(s). See Console for details.", "OK");
        }

        public static List<string> CollectIssues()
        {
            var issues = new List<string>();
            PersistentIdentity[] identities = FindSceneIdentities();
            foreach (PersistentIdentity identity in identities)
            {
                if (!identity.RuntimeSpawned && !identity.HasValidId)
                    issues.Add($"Missing stable ID at {GetPath(identity.transform)} in scene '{identity.SceneName}'.");
                if (identity.RuntimeSpawned && string.IsNullOrWhiteSpace(identity.PrefabId))
                    issues.Add($"Runtime persistent object '{GetPath(identity.transform)}' has no prefab ID.");

                string[] duplicateParticipantKeys = identity.GetParticipants()
                    .GroupBy(participant => participant.ParticipantKey, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToArray();
                foreach (string key in duplicateParticipantKeys)
                    issues.Add($"Object '{GetPath(identity.transform)}' has duplicate participant key '{key}'.");
            }

            foreach (IGrouping<string, PersistentIdentity> duplicate in identities
                         .Where(identity => identity.HasValidId)
                         .GroupBy(identity => identity.ObjectId, StringComparer.Ordinal)
                         .Where(group => group.Count() > 1))
            {
                issues.Add($"Duplicate persistent object ID '{duplicate.Key}' used by {duplicate.Count()} objects.");
            }
            return issues;
        }

        private static PersistentIdentity[] FindSceneIdentities()
        {
            var result = new List<PersistentIdentity>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects()) result.AddRange(root.GetComponentsInChildren<PersistentIdentity>(true));
            }
            return result.ToArray();
        }

        private static string GetPath(Transform transform)
        {
            var names = new Stack<string>();
            for (Transform current = transform; current != null; current = current.parent) names.Push(current.name);
            return string.Join("/", names);
        }
    }
}