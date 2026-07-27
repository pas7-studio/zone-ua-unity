using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZoneUA.Persistence;

[DisallowMultipleComponent]
public sealed class SaveGameCoordinator : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Health playerHealth;
    [SerializeField] private WeaponSwitcher weaponSwitcher;

    [Header("World Persistence")]
    [SerializeField] private PersistentPrefabCatalog persistentPrefabCatalog;
    [SerializeField] private Transform runtimePersistentRoot;

    [Header("Scene and Slots")]
    [SerializeField] private SceneBootstrapper sceneBootstrapper;
    [SerializeField] private string defaultSlot = "autosave";
    [SerializeField] private int currentWorldSeed;
    [SerializeField, Min(0f)] private float autosaveIntervalSeconds = 120f;
    [SerializeField] private bool autosaveOnApplicationPause = true;
    [SerializeField] private bool autosaveOnApplicationQuit = true;

    private SaveSlotStore store;
    private SaveGameData pendingRestore;
    private string pendingSlotId = string.Empty;
    private float autosaveTimer;

    public int CurrentWorldSeed => currentWorldSeed;
    public event Action<string> Saved;
    public event Action<string> Loaded;
    public event Action<string> SaveFailed;
    public event Action<string> LoadFailed;

    private void Awake()
    {
        store = new SaveSlotStore(Path.Combine(Application.persistentDataPath, "Saves"));
        ResolveReferences();
    }

    private void OnEnable()
    {
        if (sceneBootstrapper != null) sceneBootstrapper.SceneActivated += OnSceneActivated;
    }

    private void OnDisable()
    {
        if (sceneBootstrapper != null) sceneBootstrapper.SceneActivated -= OnSceneActivated;
    }

    private void Update()
    {
        if (autosaveIntervalSeconds <= 0f) return;
        autosaveTimer += Time.unscaledDeltaTime;
        if (autosaveTimer < autosaveIntervalSeconds) return;
        autosaveTimer = 0f;
        Save(defaultSlot);
    }

    public void SetWorldSeed(int seed) => currentWorldSeed = seed;

    [ContextMenu("Save Default Slot")]
    public void SaveDefault() => Save(defaultSlot);

    [ContextMenu("Load Default Slot")]
    public void LoadDefault() => Load(defaultSlot);

    public bool Save(string slotId)
    {
        try
        {
            ResolveReferences();
            store.Save(slotId, Capture(slotId));
            Saved?.Invoke(slotId);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            SaveFailed?.Invoke(exception.Message);
            return false;
        }
    }

    public bool Load(string slotId)
    {
        if (!store.TryLoad(slotId, out SaveGameData data))
        {
            LoadFailed?.Invoke($"Save slot '{slotId}' is missing or invalid.");
            return false;
        }

        pendingRestore = data;
        pendingSlotId = slotId;
        string currentScene = SceneManager.GetActiveScene().name;
        if (sceneBootstrapper != null && !string.IsNullOrWhiteSpace(data.activeScene) && data.activeScene != currentScene)
        {
            if (!sceneBootstrapper.LoadScene(data.activeScene))
            {
                pendingRestore = null;
                pendingSlotId = string.Empty;
                LoadFailed?.Invoke($"Could not start loading scene '{data.activeScene}'.");
                return false;
            }
            return true;
        }

        ApplyPendingRestore();
        return true;
    }

    public bool Delete(string slotId)
    {
        store.Delete(slotId);
        return !store.Exists(slotId);
    }

    private SaveGameData Capture(string slotId)
    {
        var data = new SaveGameData
        {
            activeScene = SceneManager.GetActiveScene().name,
            worldSeed = currentWorldSeed,
            player = new PlayerSaveData()
        };
        data.Stamp(slotId, DateTime.UtcNow);
        if (playerRoot != null)
        {
            data.player.position = playerRoot.position;
            data.player.rotationZ = playerRoot.eulerAngles.z;
        }
        if (playerHealth != null)
        {
            data.player.currentHealth = playerHealth.CurrentHealth;
            data.player.maximumHealth = playerHealth.MaximumHealth;
        }
        if (weaponSwitcher != null) data.player.activeWeaponIndex = weaponSwitcher.ActiveWeaponIndex;

        PersistentIdentity[] identities = FindObjectsByType<PersistentIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        data.worldObjects = PersistentWorldState.Capture(identities);
        data.destroyedObjectIds = PersistentTombstoneRegistry.Current.OrderBy(id => id, StringComparer.Ordinal).ToList();
        return data;
    }

    private void ApplyPendingRestore()
    {
        if (pendingRestore == null) return;
        ResolveReferences();
        currentWorldSeed = pendingRestore.worldSeed;
        PersistentTombstoneRegistry.Replace(pendingRestore.destroyedObjectIds);
        EnsureRuntimeObjects(pendingRestore.worldObjects);

        PersistentIdentity[] identities = FindObjectsByType<PersistentIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        PersistentRestoreReport report = PersistentWorldState.Restore(
            identities,
            pendingRestore.worldObjects,
            pendingRestore.destroyedObjectIds);
        if (report.MissingObjectIds.Count > 0)
            Debug.LogWarning($"Persistent restore could not resolve {report.MissingObjectIds.Count} object ID(s).", this);

        PlayerSaveData player = pendingRestore.player ?? new PlayerSaveData();
        if (playerRoot != null)
            playerRoot.SetPositionAndRotation(player.position, Quaternion.Euler(0f, 0f, player.rotationZ));
        if (playerHealth != null)
        {
            playerHealth.SetMaximumHealth(player.maximumHealth);
            playerHealth.SetHealth(player.currentHealth);
        }
        if (weaponSwitcher != null && player.activeWeaponIndex >= 0)
            weaponSwitcher.RequestSwitch(player.activeWeaponIndex);

        string loadedSlot = pendingSlotId;
        pendingRestore = null;
        pendingSlotId = string.Empty;
        Loaded?.Invoke(loadedSlot);
    }

    private void EnsureRuntimeObjects(IReadOnlyList<PersistentObjectSaveData> objects)
    {
        if (objects == null || persistentPrefabCatalog == null) return;
        var existing = new HashSet<string>(
            FindObjectsByType<PersistentIdentity>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(identity => identity != null && identity.HasValidId)
                .Select(identity => identity.ObjectId),
            StringComparer.Ordinal);

        foreach (PersistentObjectSaveData objectData in objects)
        {
            if (objectData == null || !objectData.runtimeSpawned || existing.Contains(objectData.objectId)) continue;
            if (PersistentTombstoneRegistry.Current.Contains(objectData.objectId)) continue;
            if (!persistentPrefabCatalog.TryGetPrefab(objectData.prefabId, out GameObject prefab))
            {
                Debug.LogWarning($"No persistent prefab registered for '{objectData.prefabId}'.", this);
                continue;
            }

            GameObject instance = Instantiate(prefab, runtimePersistentRoot);
            PersistentIdentity identity = instance.GetComponent<PersistentIdentity>() ?? instance.AddComponent<PersistentIdentity>();
            identity.AssignRuntimeId(objectData.objectId, objectData.prefabId);
            existing.Add(objectData.objectId);
        }
    }

    private void OnSceneActivated(string sceneName) => ApplyPendingRestore();

    private void ResolveReferences()
    {
        playerRoot ??= playerHealth != null ? playerHealth.transform : null;
        if (playerRoot != null)
        {
            playerHealth ??= playerRoot.GetComponent<Health>();
            weaponSwitcher ??= playerRoot.GetComponent<WeaponSwitcher>();
        }
        sceneBootstrapper ??= SceneBootstrapper.Instance;
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && autosaveOnApplicationPause) Save(defaultSlot);
    }

    private void OnApplicationQuit()
    {
        if (autosaveOnApplicationQuit) Save(defaultSlot);
    }
}