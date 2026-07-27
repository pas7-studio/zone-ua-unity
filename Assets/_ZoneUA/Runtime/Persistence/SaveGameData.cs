using System;
using UnityEngine;

namespace ZoneUA.Persistence
{
    [Serializable]
    public sealed class SaveGameData
    {
        public const int CurrentSchemaVersion = 2;

        public int schemaVersion = CurrentSchemaVersion;
        public string saveId = string.Empty;
        public string savedAtUtc = string.Empty;
        public string activeScene = string.Empty;
        public int worldSeed;
        public PlayerSaveData player = new PlayerSaveData();

        public void Stamp(string id, DateTime utcNow)
        {
            schemaVersion = CurrentSchemaVersion;
            saveId = id ?? string.Empty;
            savedAtUtc = utcNow.ToUniversalTime().ToString("O");
            player ??= new PlayerSaveData();
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
}
