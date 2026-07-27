using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ZoneUA.Persistence
{
    [Serializable]
    public sealed class SaveEnvelope
    {
        public string checksum = string.Empty;
        public string payload = string.Empty;
    }

    public sealed class SaveSlotStore
    {
        private readonly string rootDirectory;

        public SaveSlotStore(string rootDirectory)
        {
            this.rootDirectory = string.IsNullOrWhiteSpace(rootDirectory)
                ? throw new ArgumentException("A save root directory is required.", nameof(rootDirectory))
                : rootDirectory;
        }

        public string GetSlotPath(string slotId) => Path.Combine(rootDirectory, Sanitize(slotId) + ".json");
        public string GetBackupPath(string slotId) => GetSlotPath(slotId) + ".bak";

        public void Save(string slotId, SaveGameData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Directory.CreateDirectory(rootDirectory);

            string payload = JsonUtility.ToJson(data, false);
            var envelope = new SaveEnvelope { payload = payload, checksum = ComputeChecksum(payload) };
            string serialized = JsonUtility.ToJson(envelope, true);
            string path = GetSlotPath(slotId);
            string temp = path + ".tmp";
            string backup = GetBackupPath(slotId);

            File.WriteAllText(temp, serialized, Encoding.UTF8);
            if (File.Exists(path)) File.Copy(path, backup, true);
            if (File.Exists(path)) File.Delete(path);
            File.Move(temp, path);
        }

        public bool TryLoad(string slotId, out SaveGameData data)
        {
            if (TryLoadPath(GetSlotPath(slotId), out data)) return true;
            return TryLoadPath(GetBackupPath(slotId), out data);
        }

        public bool Exists(string slotId) => File.Exists(GetSlotPath(slotId)) || File.Exists(GetBackupPath(slotId));

        public void Delete(string slotId)
        {
            DeleteIfExists(GetSlotPath(slotId));
            DeleteIfExists(GetBackupPath(slotId));
            DeleteIfExists(GetSlotPath(slotId) + ".tmp");
        }

        private static bool TryLoadPath(string path, out SaveGameData data)
        {
            data = null;
            if (!File.Exists(path)) return false;
            try
            {
                SaveEnvelope envelope = JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path, Encoding.UTF8));
                if (envelope == null || string.IsNullOrEmpty(envelope.payload)) return false;
                if (!string.Equals(envelope.checksum, ComputeChecksum(envelope.payload), StringComparison.OrdinalIgnoreCase)) return false;
                data = SaveGameMigrator.Migrate(JsonUtility.FromJson<SaveGameData>(envelope.payload));
                return data != null;
            }
            catch
            {
                data = null;
                return false;
            }
        }

        private static string ComputeChecksum(string value)
        {
            using SHA256 sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
            return Convert.ToHexString(bytes);
        }

        private static string Sanitize(string slotId)
        {
            string value = string.IsNullOrWhiteSpace(slotId) ? "autosave" : slotId.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return value;
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
