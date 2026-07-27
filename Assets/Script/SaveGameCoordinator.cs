using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZoneUA.Persistence;

[DisallowMultipleComponent]
public sealed class SaveGameCoordinator : MonoBehaviour
{
    [SerializeField] private Transform playerRoot;
    [SerializeField] private Health playerHealth;
    [SerializeField] private WeaponSwitcher weaponSwitcher;
    [SerializeField] private SceneBootstrapper sceneBootstrapper;
    [SerializeField] private string defaultSlot = "autosave";
    [SerializeField, Min(0f)] private float autosaveIntervalSeconds = 120f;
    [SerializeField] private bool autosaveOnApplicationPause = true;
    [SerializeField] private bool autosaveOnApplicationQuit = true;

    private SaveSlotStore store;
    private SaveGameData pendingRestore;
    private float autosaveTimer;

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

    [ContextMenu("Save Default Slot")]
    public void SaveDefault() => Save(defaultSlot);

    [ContextMenu("Load Default Slot")]
    public void LoadDefault() => Load(defaultSlot);

    public bool Save(string slotId)
    {
        try
        {
            ResolveReferences();
            SaveGameData data = Capture(slotId);
            store.Save(slotId, data);
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
        string currentScene = SceneManager.GetActiveScene().name;
        if (sceneBootstrapper != null && !string.IsNullOrWhiteSpace(data.activeScene) && data.activeScene != currentScene)
        {
            sceneBootstrapper.LoadScene(data.activeScene);
            return true;
        }

        ApplyPendingRestore(slotId);
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
        return data;
    }

    private void ApplyPendingRestore(string slotId)
    {
        if (pendingRestore == null) return;
        ResolveReferences();
        PlayerSaveData player = pendingRestore.player ?? new PlayerSaveData();
        if (playerRoot != null)
        {
            playerRoot.SetPositionAndRotation(player.position, Quaternion.Euler(0f, 0f, player.rotationZ));
        }
        if (playerHealth != null)
        {
            playerHealth.SetMaximumHealth(player.maximumHealth);
            playerHealth.SetHealth(player.currentHealth);
        }
        if (weaponSwitcher != null && player.activeWeaponIndex >= 0)
        {
            weaponSwitcher.RequestSwitch(player.activeWeaponIndex);
        }
        pendingRestore = null;
        Loaded?.Invoke(slotId);
    }

    private void OnSceneActivated(string sceneName) => ApplyPendingRestore(defaultSlot);

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
