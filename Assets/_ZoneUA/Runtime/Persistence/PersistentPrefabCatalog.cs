using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneUA.Persistence
{
    [CreateAssetMenu(menuName = "Zone UA/Persistence/Persistent Prefab Catalog", fileName = "PersistentPrefabCatalog")]
    public sealed class PersistentPrefabCatalog : ScriptableObject
    {
        [Serializable]
        private struct Entry
        {
            public string prefabId;
            public GameObject prefab;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();
        private Dictionary<string, GameObject> cache;

        public bool TryGetPrefab(string prefabId, out GameObject prefab)
        {
            EnsureCache();
            return cache.TryGetValue(prefabId ?? string.Empty, out prefab) && prefab != null;
        }

        public IReadOnlyList<string> ValidateEntries()
        {
            var issues = new List<string>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (Entry entry in entries)
            {
                string id = entry.prefabId?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(id)) issues.Add("Persistent prefab catalog contains an empty prefab ID.");
                else if (!ids.Add(id)) issues.Add($"Duplicate persistent prefab ID '{id}'.");
                if (entry.prefab == null) issues.Add($"Persistent prefab '{id}' has no prefab reference.");
            }
            return issues;
        }

        private void EnsureCache()
        {
            if (cache != null) return;
            cache = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            foreach (Entry entry in entries)
            {
                string id = entry.prefabId?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(id) && entry.prefab != null && !cache.ContainsKey(id)) cache.Add(id, entry.prefab);
            }
        }

        private void OnValidate() => cache = null;
    }
}