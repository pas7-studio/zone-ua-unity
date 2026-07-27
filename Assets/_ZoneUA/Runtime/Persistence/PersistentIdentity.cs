using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZoneUA.Persistence
{
    [DisallowMultipleComponent]
    public sealed class PersistentIdentity : MonoBehaviour
    {
        [SerializeField, Tooltip("Stable ID for a scene object. Runtime-spawned objects receive an instance ID when created.")]
        private string objectId = string.Empty;
        [SerializeField, Tooltip("Stable content ID used to recreate runtime-spawned objects.")]
        private string prefabId = string.Empty;
        [SerializeField] private bool runtimeSpawned;

        public string ObjectId => objectId;
        public string PrefabId => prefabId;
        public bool RuntimeSpawned => runtimeSpawned;
        public string SceneName => gameObject.scene.IsValid() ? gameObject.scene.name : string.Empty;
        public bool HasValidId => !string.IsNullOrWhiteSpace(objectId);

        public void AssignSceneId(string value)
        {
            if (runtimeSpawned) throw new InvalidOperationException("Runtime-spawned identities cannot be assigned as scene identities.");
            objectId = Normalize(value);
        }

        public void AssignRuntimeId(string value, string sourcePrefabId)
        {
            runtimeSpawned = true;
            objectId = Normalize(value);
            prefabId = Normalize(sourcePrefabId);
        }

        public void EnsureRuntimeId(string sourcePrefabId)
        {
            if (!runtimeSpawned || string.IsNullOrWhiteSpace(objectId))
                AssignRuntimeId(Guid.NewGuid().ToString("N"), sourcePrefabId);
        }

        public IReadOnlyList<IPersistentSaveParticipant> GetParticipants()
        {
            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            var result = new List<IPersistentSaveParticipant>();
            foreach (MonoBehaviour behaviour in behaviours)
                if (behaviour is IPersistentSaveParticipant participant) result.Add(participant);
            result.Sort((left, right) => string.CompareOrdinal(left.ParticipantKey, right.ParticipantKey));
            return result;
        }

        private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}