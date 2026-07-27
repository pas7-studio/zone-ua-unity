using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ZoneUA.EditorValidation
{
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    [Serializable]
    public sealed class ValidationIssue
    {
        public ValidationSeverity severity;
        public string code;
        public string message;
        public string assetPath;

        public ValidationIssue(ValidationSeverity severity, string code, string message, string assetPath = "")
        {
            this.severity = severity;
            this.code = code;
            this.message = message;
            this.assetPath = assetPath ?? string.Empty;
        }
    }

    [Serializable]
    public sealed class ValidationReport
    {
        public string generatedAtUtc;
        public List<ValidationIssue> issues = new List<ValidationIssue>();

        public int ErrorCount => issues.Count(issue => issue.severity == ValidationSeverity.Error);
        public int WarningCount => issues.Count(issue => issue.severity == ValidationSeverity.Warning);
        public bool IsValid => ErrorCount == 0;

        public void Add(ValidationSeverity severity, string code, string message, string assetPath = "")
        {
            issues.Add(new ValidationIssue(severity, code, message, assetPath));
        }
    }

    public static class ZoneUAProjectValidator
    {
        private const string InputActionsPath = "Assets/_ZoneUA/Input/ZoneUAInput.inputactions";
        private static readonly string[] RequiredInputActions =
        {
            "Move", "Look", "Sprint", "Fire", "Reload",
            "SwitchFireMode", "Weapon1", "Weapon2", "HideWeapon"
        };

        [MenuItem("Zone UA/Validation/Validate Project", priority = 1)]
        public static void ValidateProjectMenu()
        {
            ValidationReport report = ValidateProject();
            LogReport(report);
            ZoneUAValidationWindow.ShowReport(report);
        }

        [MenuItem("Zone UA/Validation/Validate Selected Prefabs", priority = 2)]
        public static void ValidateSelectedPrefabsMenu()
        {
            ValidationReport report = CreateReport();
            foreach (UnityEngine.Object selected in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selected);
                if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    ValidatePrefab(path, report);
                }
            }

            LogReport(report);
            ZoneUAValidationWindow.ShowReport(report);
        }

        public static ValidationReport ValidateProject()
        {
            ValidationReport report = CreateReport();
            ValidateMetaFiles(report);
            ValidateDuplicateGuids(report);
            ValidateAssemblyDefinitions(report);
            ValidateInputActions(report);
            ValidateBuildScenes(report);
            ValidateDefinitions(report);
            ValidatePrefabs(report);
            ValidateScenes(report);
            return report;
        }

        public static void WriteJsonReport(ValidationReport report, string outputPath)
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(outputPath, JsonUtility.ToJson(report, true));
        }

        private static ValidationReport CreateReport()
        {
            return new ValidationReport
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O")
            };
        }

        private static void ValidateMetaFiles(ValidationReport report)
        {
            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.EndsWith(".meta", StringComparison.Ordinal))
                {
                    continue;
                }

                if (!File.Exists(path + ".meta"))
                {
                    report.Add(ValidationSeverity.Error, "META_MISSING", "Asset or folder has no .meta file.", path);
                }
            }
        }

        private static void ValidateDuplicateGuids(ValidationReport report)
        {
            Dictionary<string, string> pathByGuid = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string metaPath in Directory.GetFiles("Assets", "*.meta", SearchOption.AllDirectories))
            {
                string guid = ReadMetaGuid(metaPath);
                if (string.IsNullOrEmpty(guid))
                {
                    report.Add(ValidationSeverity.Error, "META_GUID_MISSING", "Meta file has no GUID.", metaPath.Replace('\\', '/'));
                    continue;
                }

                string normalisedPath = metaPath.Replace('\\', '/');
                string existingPath;
                if (pathByGuid.TryGetValue(guid, out existingPath))
                {
                    report.Add(
                        ValidationSeverity.Error,
                        "META_GUID_DUPLICATE",
                        "Duplicate GUID also used by " + existingPath + ".",
                        normalisedPath);
                }
                else
                {
                    pathByGuid.Add(guid, normalisedPath);
                }
            }
        }

        private static string ReadMetaGuid(string path)
        {
            foreach (string line in File.ReadLines(path))
            {
                if (line.StartsWith("guid: ", StringComparison.Ordinal))
                {
                    return line.Substring(6).Trim();
                }
            }

            return string.Empty;
        }

        private static void ValidateAssemblyDefinitions(ValidationReport report)
        {
            Dictionary<string, string> pathByName = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string path in Directory.GetFiles("Assets", "*.asmdef", SearchOption.AllDirectories))
            {
                string normalisedPath = path.Replace('\\', '/');
                try
                {
                    AssemblyDefinitionData data = JsonUtility.FromJson<AssemblyDefinitionData>(File.ReadAllText(path));
                    if (data == null || string.IsNullOrWhiteSpace(data.name))
                    {
                        report.Add(ValidationSeverity.Error, "ASMDEF_NAME_MISSING", "Assembly definition has no name.", normalisedPath);
                        continue;
                    }

                    string existingPath;
                    if (pathByName.TryGetValue(data.name, out existingPath))
                    {
                        report.Add(
                            ValidationSeverity.Error,
                            "ASMDEF_NAME_DUPLICATE",
                            "Assembly name is already used by " + existingPath + ".",
                            normalisedPath);
                    }
                    else
                    {
                        pathByName.Add(data.name, normalisedPath);
                    }
                }
                catch (Exception exception)
                {
                    report.Add(ValidationSeverity.Error, "ASMDEF_INVALID_JSON", exception.Message, normalisedPath);
                }
            }
        }

        private static void ValidateInputActions(ValidationReport report)
        {
            if (!File.Exists(InputActionsPath))
            {
                report.Add(ValidationSeverity.Error, "INPUT_ACTIONS_MISSING", "The central InputActionAsset is missing.", InputActionsPath);
                return;
            }

            string content = File.ReadAllText(InputActionsPath);
            if (content.IndexOf("\"name\": \"Player\"", StringComparison.Ordinal) < 0)
            {
                report.Add(ValidationSeverity.Error, "INPUT_MAP_MISSING", "Input action map 'Player' is missing.", InputActionsPath);
            }

            foreach (string action in RequiredInputActions)
            {
                if (content.IndexOf("\"name\": \"" + action + "\"", StringComparison.Ordinal) < 0)
                {
                    report.Add(ValidationSeverity.Error, "INPUT_ACTION_MISSING", "Required input action is missing: " + action + ".", InputActionsPath);
                }
            }
        }

        private static void ValidateBuildScenes(ValidationReport report)
        {
            EditorBuildSettingsScene[] enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled && !string.IsNullOrWhiteSpace(scene.path)).ToArray();
            if (enabledScenes.Length == 0)
            {
                report.Add(ValidationSeverity.Warning, "BUILD_SCENES_EMPTY", "No enabled scenes are configured in Build Settings.", "ProjectSettings/EditorBuildSettings.asset");
            }
        }

        private static void ValidateDefinitions(ValidationReport report)
        {
            ValidateScriptableObjectsByTypeName("BiomeDefinition", ValidateBiomeDefinition, report);
            ValidateScriptableObjectsByTypeName("WorldGenerationSettings", ValidateWorldGenerationSettings, report);
            ValidateScriptableObjectsByTypeName("WeaponDefinition", ValidateIdProperty, report);
            ValidateScriptableObjectsByTypeName("FactionDefinition", ValidateIdProperty, report);
        }

        private static void ValidateScriptableObjectsByTypeName(
            string typeName,
            Action<SerializedObject, string, ValidationReport> validator,
            ValidationReport report)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null || asset.GetType().Name != typeName)
                {
                    continue;
                }

                validator(new SerializedObject(asset), path, report);
            }
        }

        private static void ValidateIdProperty(SerializedObject serialized, string path, ValidationReport report)
        {
            SerializedProperty id = serialized.FindProperty("id");
            if (id != null && string.IsNullOrWhiteSpace(id.stringValue))
            {
                report.Add(ValidationSeverity.Error, "DEFINITION_ID_EMPTY", "Definition has an empty id.", path);
            }
        }

        private static void ValidateBiomeDefinition(SerializedObject serialized, string path, ValidationReport report)
        {
            ValidateIdProperty(serialized, path, report);
            SerializedProperty terrainPrefab = serialized.FindProperty("terrainPrefab");
            if (terrainPrefab != null && terrainPrefab.objectReferenceValue == null)
            {
                report.Add(ValidationSeverity.Warning, "BIOME_TERRAIN_MISSING", "Biome has no terrain presentation prefab.", path);
            }
        }

        private static void ValidateWorldGenerationSettings(SerializedObject serialized, string path, ValidationReport report)
        {
            SerializedProperty fallbackBiome = serialized.FindProperty("fallbackBiome");
            if (fallbackBiome != null && fallbackBiome.objectReferenceValue == null)
            {
                report.Add(ValidationSeverity.Error, "WORLD_FALLBACK_MISSING", "World settings have no fallback biome.", path);
            }

            SerializedProperty biomes = serialized.FindProperty("biomes");
            if (biomes == null || !biomes.isArray || biomes.arraySize == 0)
            {
                report.Add(ValidationSeverity.Error, "WORLD_BIOMES_EMPTY", "World settings contain no biome list.", path);
            }
        }

        private static void ValidatePrefabs(ValidationReport report)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab"))
            {
                ValidatePrefab(AssetDatabase.GUIDToAssetPath(guid), report);
            }
        }

        private static void ValidatePrefab(string path, ValidationReport report)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return;
            }

            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            HashSet<string> typeNames = new HashSet<string>(behaviours.Where(item => item != null).Select(item => item.GetType().Name));

            if (typeNames.Contains("NPCController"))
            {
                RequireComponentType(typeNames, "Health", "NPC_HEALTH_MISSING", path, report);
                RequireComponentType(typeNames, "Death", "NPC_DEATH_MISSING", path, report);
                RequireComponentType(typeNames, "FactionMember", "NPC_FACTION_MISSING", path, report);
            }

            if (typeNames.Contains("WeaponController") && !typeNames.Contains("Weapon"))
            {
                report.Add(ValidationSeverity.Error, "WEAPON_DATA_MISSING", "WeaponController prefab has no Weapon component.", path);
            }

            if (typeNames.Contains("PlayerInputRouter"))
            {
                MonoBehaviour router = behaviours.FirstOrDefault(item => item != null && item.GetType().Name == "PlayerInputRouter");
                SerializedObject serialized = new SerializedObject(router);
                SerializedProperty actions = serialized.FindProperty("actions");
                if (actions != null && actions.objectReferenceValue == null)
                {
                    report.Add(ValidationSeverity.Error, "PLAYER_ACTIONS_UNASSIGNED", "PlayerInputRouter has no InputActionAsset assigned.", path);
                }
            }
        }

        private static void RequireComponentType(
            HashSet<string> typeNames,
            string requiredType,
            string code,
            string path,
            ValidationReport report)
        {
            if (!typeNames.Contains(requiredType))
            {
                report.Add(ValidationSeverity.Error, code, "NPC prefab requires " + requiredType + ".", path);
            }
        }

        private static void ValidateScenes(ValidationReport report)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!File.Exists(path + ".meta"))
                {
                    report.Add(ValidationSeverity.Error, "SCENE_META_MISSING", "Scene has no .meta file.", path);
                }
            }
        }

        public static void LogReport(ValidationReport report)
        {
            foreach (ValidationIssue issue in report.issues)
            {
                string text = "[ZoneUA:" + issue.code + "] " + issue.message +
                              (string.IsNullOrEmpty(issue.assetPath) ? string.Empty : " (" + issue.assetPath + ")");
                switch (issue.severity)
                {
                    case ValidationSeverity.Error:
                        Debug.LogError(text);
                        break;
                    case ValidationSeverity.Warning:
                        Debug.LogWarning(text);
                        break;
                    default:
                        Debug.Log(text);
                        break;
                }
            }

            Debug.Log("Zone UA validation finished: " + report.ErrorCount + " error(s), " + report.WarningCount + " warning(s).");
        }

        [Serializable]
        private sealed class AssemblyDefinitionData
        {
            public string name;
        }
    }

    public sealed class ZoneUABuildValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport buildReport)
        {
            ValidationReport report = ZoneUAProjectValidator.ValidateProject();
            ZoneUAProjectValidator.WriteJsonReport(report, "Logs/ZoneUAValidation.json");
            ZoneUAProjectValidator.LogReport(report);
            if (!report.IsValid)
            {
                throw new BuildFailedException("Zone UA project validation failed with " + report.ErrorCount + " error(s). See Logs/ZoneUAValidation.json.");
            }
        }
    }

    public static class ZoneUAValidationCli
    {
        public static void Run()
        {
            ValidationReport report = ZoneUAProjectValidator.ValidateProject();
            ZoneUAProjectValidator.WriteJsonReport(report, "Logs/ZoneUAValidation.json");
            ZoneUAProjectValidator.LogReport(report);
            if (!report.IsValid)
            {
                throw new Exception("Zone UA validation failed with " + report.ErrorCount + " error(s).");
            }
        }
    }

    public sealed class ZoneUAValidationWindow : EditorWindow
    {
        private ValidationReport report;
        private Vector2 scroll;

        public static void ShowReport(ValidationReport report)
        {
            ZoneUAValidationWindow window = GetWindow<ZoneUAValidationWindow>("Zone UA Validation");
            window.report = report;
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Project Validation", EditorStyles.boldLabel);
            if (GUILayout.Button("Run Validation"))
            {
                report = ZoneUAProjectValidator.ValidateProject();
                ZoneUAProjectValidator.LogReport(report);
            }

            if (report == null)
            {
                EditorGUILayout.HelpBox("Run validation to inspect project issues.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Errors", report.ErrorCount.ToString());
            EditorGUILayout.LabelField("Warnings", report.WarningCount.ToString());
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (ValidationIssue issue in report.issues)
            {
                MessageType messageType = issue.severity == ValidationSeverity.Error
                    ? MessageType.Error
                    : issue.severity == ValidationSeverity.Warning ? MessageType.Warning : MessageType.Info;
                EditorGUILayout.HelpBox(issue.code + ": " + issue.message + "\n" + issue.assetPath, messageType);
                if (!string.IsNullOrEmpty(issue.assetPath) && GUILayout.Button("Select " + issue.assetPath))
                {
                    Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(issue.assetPath);
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
