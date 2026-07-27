using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneUA.Persistence
{
    public static class PersistentWorldState
    {
        public static List<PersistentObjectSaveData> Capture(IEnumerable<PersistentIdentity> identities)
        {
            var result = new List<PersistentObjectSaveData>();
            if (identities == null) return result;

            foreach (PersistentIdentity identity in identities
                         .Where(item => item != null && item.HasValidId)
                         .OrderBy(item => item.ObjectId, StringComparer.Ordinal))
            {
                var objectData = new PersistentObjectSaveData
                {
                    objectId = identity.ObjectId,
                    sceneName = identity.SceneName,
                    prefabId = identity.PrefabId,
                    runtimeSpawned = identity.RuntimeSpawned
                };

                foreach (IPersistentSaveParticipant participant in identity.GetParticipants()
                             .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ParticipantKey))
                             .GroupBy(item => item.ParticipantKey, StringComparer.Ordinal)
                             .Select(group => group.First()))
                {
                    objectData.components.Add(new PersistentComponentSaveData
                    {
                        participantKey = participant.ParticipantKey,
                        version = Math.Max(1, participant.ParticipantVersion),
                        payload = participant.CaptureState() ?? string.Empty
                    });
                }

                result.Add(objectData);
            }

            return result;
        }

        public static PersistentRestoreReport Restore(
            IEnumerable<PersistentIdentity> identities,
            IReadOnlyList<PersistentObjectSaveData> savedObjects,
            IReadOnlyCollection<string> tombstones)
        {
            var report = new PersistentRestoreReport();
            var byId = identities?
                .Where(item => item != null && item.HasValidId)
                .GroupBy(item => item.ObjectId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal)
                ?? new Dictionary<string, PersistentIdentity>(StringComparer.Ordinal);

            if (tombstones != null)
            {
                foreach (string id in tombstones.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal))
                {
                    if (!byId.TryGetValue(id, out PersistentIdentity identity)) continue;
                    identity.gameObject.SetActive(false);
                    report.DestroyedObjectsApplied++;
                }
            }

            if (savedObjects == null) return report;
            foreach (PersistentObjectSaveData objectData in savedObjects)
            {
                if (objectData == null || string.IsNullOrWhiteSpace(objectData.objectId)) continue;
                if (!byId.TryGetValue(objectData.objectId, out PersistentIdentity identity))
                {
                    report.MissingObjectIds.Add(objectData.objectId);
                    continue;
                }

                var participants = identity.GetParticipants()
                    .Where(item => item != null && !string.IsNullOrWhiteSpace(item.ParticipantKey))
                    .GroupBy(item => item.ParticipantKey, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
                foreach (PersistentComponentSaveData component in objectData.components ?? new List<PersistentComponentSaveData>())
                {
                    if (component == null || string.IsNullOrWhiteSpace(component.participantKey)) continue;
                    if (!participants.TryGetValue(component.participantKey, out IPersistentSaveParticipant participant))
                    {
                        report.MissingParticipantKeys.Add($"{objectData.objectId}:{component.participantKey}");
                        continue;
                    }
                    participant.RestoreState(component.payload ?? string.Empty, component.version);
                    report.ComponentsRestored++;
                }
                report.ObjectsRestored++;
            }
            return report;
        }
    }

    public sealed class PersistentRestoreReport
    {
        public int ObjectsRestored { get; internal set; }
        public int ComponentsRestored { get; internal set; }
        public int DestroyedObjectsApplied { get; internal set; }
        public List<string> MissingObjectIds { get; } = new List<string>();
        public List<string> MissingParticipantKeys { get; } = new List<string>();
    }
}