using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneUA.Persistence
{
    [Serializable]
    public sealed class SaveGameData
    {
        public const int CurrentSchemaVersion = 3;

        public int schemaVersion = CurrentSchemaVersion;
        public string saveId = string.Empty;
        public string savedAtUtc = string.Empty;
        public string activeScene = string.Empty;
        public int worldSeed;
        public PlayerSaveData player = new PlayerSaveData();
        public List<PersistentObjectSaveData> worldObjects = new List<PersistentObjectSaveData>();
        public List<string> destroyedObjectIds = new List<string>();

        public void Stamp(string id, DateTime utcNow)
        {
            schemaVersion = CurrentSchemaVersion;
            saveId = id ?? string.Empty;
            savedAtUtc = utcNow.ToUniversalTime().ToString("O");
            player ??= new PlayerSaveData();
            worldObjects ??= new List<PersistentObjectSaveData>();
            destroyedObjectIds ??= new List<string>();
        }
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public Vector3 position;
        public float rotationZ;
        public int currentHealth = 100;
        public int maximumHealth = 100;
        public int activeWeaponIndex = -1;
    }

    [Serializable]
    public sealed class PersistentObjectSaveData
    {
        public string objectId = string.Empty;
        public string sceneName = string.Empty;
        public string prefabId = string.Empty;
        public bool runtimeSpawned;
        public List<PersistentComponentSaveData> components = new List<PersistentComponentSaveData>();
    }

    [Serializable]
    public sealed class PersistentComponentSaveData
    {
        public string participantKey = string.Empty;
        public int version = 1;
        public string payload = string.Empty;
    }
}