using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZoneUA.EditorValidation
{
    public static class ZoneUAIntegrationRunner
    {
        private const string SourceScenePath = "Assets/Scenes/SampleScene.unity";
        private const string ProductionScenePath = ZoneUASceneArchitectureTools.ProductionScenePath;
        private const string InputActionsPath = "Assets/_ZoneUA/Input/ZoneUAInput.inputactions";

        [MenuItem("Zone UA/Integration/Integrate Production Scene", priority = 1)]
        public static void IntegrateProductionScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath) == null)
                throw new InvalidOperationException($"Missing source scene: {SourceScenePath}");

            Scene scene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ProductionScenePath);
            scene = SceneManager.GetActiveScene();

            GameObject player = FindRoot("MainPlayer");
            if (player == null) throw new InvalidOperationException("SampleScene does not contain MainPlayer.");

            AddComponent(player, "PersistentIdentity");
            AddComponent(player, "TransformSaveParticipant");
            AddComponent(player, "HealthSaveParticipant");
            Component inventory = AddComponent(player, "InventoryComponent");
            SetInt(inventory, "capacity", 24);
            AddComponent(player, "InventorySaveParticipant");

            UnityEngine.Object actions = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(InputActionsPath);
            Component input = AddComponent(player, "PlayerInputRouter");
            SetObject(input, "actions", actions);
            SetObject(input, "characterController", FindComponent(player, "CharacterCustomController"));
            SetObject(input, "weaponSwitcher", FindComponent(player, "WeaponSwitcher"));

            foreach (string npcName in new[] { "Solder_Human", "Lutj_Mutant", "Scientiest_Human" })
            {
                GameObject npc = FindRoot(npcName);
                if (npc == null) continue;
                AddComponent(npc, "PersistentIdentity");
                AddComponent(npc, "TransformSaveParticipant");
                AddComponent(npc, "HealthSaveParticipant");
            }

            GameObject services = FindRoot("ZoneUA_Persistence") ?? new GameObject("ZoneUA_Persistence");
            SceneManager.MoveGameObjectToScene(services, scene);
            Component save = AddComponent(services, "SaveGameCoordinator");
            SetObject(save, "playerRoot", player.transform);
            SetObject(save, "playerHealth", FindComponent(player, "Health"));
            SetObject(save, "weaponSwitcher", FindComponent(player, "WeaponSwitcher"));
            SetFloat(save, "autosaveIntervalSeconds", 0f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ProductionScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Zone UA production integration completed: {ProductionScenePath}");
        }

        [MenuItem("Zone UA/Integration/Integrate Production Prefabs", priority = 2)]
        public static void IntegrateProductionPrefabs()
        {
            string[] paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.StartsWith("Assets/", StringComparison.Ordinal) &&
                               !path.StartsWith("Assets/ThirdParty/", StringComparison.Ordinal) &&
                               !path.Contains("/Samples~/", StringComparison.Ordinal) &&
                               !path.Contains("/Tests/", StringComparison.Ordinal))
                .ToArray();

            CompositionMigrationReport report = ZoneUAProductionMigrator.AuditAssets(paths, true);
            ConfigurePlayerPrefab("Assets/Prefabs/MainPlayer.prefab");
            AssetDatabase.SaveAssets();
            Debug.Log($"Zone UA prefab integration completed: added={report.AddedCount}, missing={report.MissingCount}, errors={report.ErrorCount}");
        }

        private static void ConfigurePlayerPrefab(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Component input = AddComponent(root, "PlayerInputRouter");
                UnityEngine.Object actions = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(InputActionsPath);
                SetObject(input, "actions", actions);
                SetObject(input, "characterController", FindComponent(root, "CharacterCustomController"));
                SetObject(input, "weaponSwitcher", FindComponent(root, "WeaponSwitcher"));
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static GameObject FindRoot(string name) =>
            SceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(root => root.name == name);

        private static Component AddComponent(GameObject target, string typeName)
        {
            Type type = FindType(typeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
                throw new InvalidOperationException($"Could not resolve component type '{typeName}'.");
            return target.GetComponent(type) ?? Undo.AddComponent(target, type);
        }

        private static Component FindComponent(GameObject target, string typeName)
        {
            Type type = FindType(typeName);
            return type == null ? null : target.GetComponent(type);
        }

        private static Type FindType(string typeName)
        {
            string[] candidates = { typeName, "ZoneUA.Persistence." + typeName, "ZoneUA.Inventory." + typeName };
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => candidates.Select(candidate => assembly.GetType(candidate, false)))
                .FirstOrDefault(type => type != null);
        }

        private static void SetObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) throw new InvalidOperationException($"Missing serialized field '{propertyName}' on {target.GetType().Name}.");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(UnityEngine.Object target, string propertyName, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) throw new InvalidOperationException($"Missing serialized field '{propertyName}' on {target.GetType().Name}.");
            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) throw new InvalidOperationException($"Missing serialized field '{propertyName}' on {target.GetType().Name}.");
            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
