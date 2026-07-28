using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZoneUA.Testing;

public static class ZoneUATestSceneBuilder
{
    private const string Root = "Assets/_ZoneUA/Scenes/Tests/";

    [MenuItem("Zone UA/Tests/Build Isolated Test Scenes", priority = 60)]
    public static void BuildAll()
    {
        EnsureFolder();
        BuildScene("WorldGenerationTestScene", "world-generation", AddWorldGenerationSetup);
        BuildScene("PlayerMovementTestScene", "player-movement", AddPlayerSetup);
        BuildScene("CombatTestScene", "combat-player-vs-npc", AddCombatSetup);
        BuildScene("NpcCombatTestScene", "combat-npc-vs-npc", AddNpcCombatSetup);
        ConfigureBuildScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Zone UA isolated test scenes rebuilt.");
    }

    private static void ConfigureBuildScenes()
    {
        string[] paths =
        {
            "Assets/_ZoneUA/Scenes/Bootstrap/Bootstrap.unity",
            "Assets/_ZoneUA/Scenes/Production/MainScene.unity",
            "Assets/_ZoneUA/Scenes/Tests/WorldGenerationTestScene.unity",
            "Assets/_ZoneUA/Scenes/Tests/PlayerMovementTestScene.unity",
            "Assets/_ZoneUA/Scenes/Tests/CombatTestScene.unity",
            "Assets/_ZoneUA/Scenes/Tests/NpcCombatTestScene.unity",
            "Assets/_ZoneUA/Scenes/Development/Development.unity",
            "Assets/_ZoneUA/Scenes/Tests/Tests.unity"
        };
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(paths[0], true),
            new EditorBuildSettingsScene(paths[1], true),
            new EditorBuildSettingsScene(paths[2], true),
            new EditorBuildSettingsScene(paths[3], true),
            new EditorBuildSettingsScene(paths[4], true),
            new EditorBuildSettingsScene(paths[5], true),
            new EditorBuildSettingsScene(paths[6], false),
            new EditorBuildSettingsScene(paths[7], false)
        };
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_ZoneUA/Scenes/Tests"))
        {
            AssetDatabase.CreateFolder("Assets/_ZoneUA/Scenes", "Tests");
        }
    }

    private static void BuildScene(string sceneName, string scenarioId, System.Action setup)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("TestScenario_" + scenarioId);
        ZoneUATestScenarioMarker marker = root.AddComponent<ZoneUATestScenarioMarker>();
        marker.Configure(scenarioId);
        setup();
        AddCameraAndLight();
        EditorSceneManager.SaveScene(scene, Root + sceneName + ".unity");
        Object.DestroyImmediate(root);
    }

    private static void AddWorldGenerationSetup()
    {
        InstantiatePrefab("Assets/Prefabs/World/MapGeneratorTest.prefab", "WorldGenerator");
        AddAnchor("WorldGenerationAnchor", new Vector3(4f, 4f, 0f));
    }

    private static void AddPlayerSetup()
    {
        InstantiatePrefab("Assets/Prefabs/GlobalSystem.prefab", "TestGlobalSystem");
        InstantiatePrefab("Assets/Prefabs/MainPlayer.prefab", "TestPlayer").transform.position = Vector3.zero;
        AddAnchor("MovementStart", Vector3.zero);
        AddAnchor("MovementTarget", new Vector3(4f, 0f, 0f));
    }

    private static void AddCombatSetup()
    {
        InstantiatePrefab("Assets/Prefabs/GlobalSystem.prefab", "TestGlobalSystem");
        InstantiatePrefab("Assets/Prefabs/MainPlayer.prefab", "TestPlayer").transform.position = new Vector3(-4f, 0f, 0f);
        InstantiatePrefab("Assets/Prefabs/Weapons/AK12.prefab", "TestWeapon").transform.position = new Vector3(-4f, 0f, 0f);
        InstantiatePrefab("Assets/Prefabs/NPC/Solder_Human.prefab", "TestNpcTarget").transform.position = new Vector3(4f, 0f, 0f);
        AddAnchor("PlayerFireLine", new Vector3(-4f, 0f, 0f));
        AddAnchor("NpcFireLine", new Vector3(4f, 0f, 0f));
    }

    private static void AddNpcCombatSetup()
    {
        InstantiatePrefab("Assets/Prefabs/NPC/Solder_Human.prefab", "NpcAlpha").transform.position = new Vector3(-3f, 0f, 0f);
        InstantiatePrefab("Assets/Prefabs/NPC/Scientiest_Human.prefab", "NpcBravo").transform.position = new Vector3(3f, 0f, 0f);
        AddAnchor("NpcAlphaLine", new Vector3(-3f, 0f, 0f));
        AddAnchor("NpcBravoLine", new Vector3(3f, 0f, 0f));
    }

    private static GameObject InstantiatePrefab(string path, string name)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError("Test scene builder could not find prefab: " + path);
            return new GameObject(name);
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        return instance;
    }

    private static void AddAnchor(string name, Vector3 position)
    {
        GameObject anchor = new GameObject(name);
        anchor.transform.position = position;
    }

    private static void AddCameraAndLight()
    {
        GameObject cameraObject = new GameObject("Test Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -20f);
        camera.orthographic = true;
        camera.orthographicSize = 12f;

        GameObject lightObject = new GameObject("Test Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

}
