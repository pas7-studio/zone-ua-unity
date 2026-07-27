using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ZoneUA.Persistence;

namespace ZoneUA.Combat.Tests
{
    public sealed class TestPersistentParticipant : MonoBehaviour, IPersistentSaveParticipant
    {
        public int value;
        public string ParticipantKey => "test";
        public int ParticipantVersion => 2;
        public string CaptureState() => value.ToString();
        public void RestoreState(string payload, int version) => value = int.Parse(payload);
    }

    public sealed class PersistentWorldStateTests
    {
        [SetUp]
        public void SetUp() => PersistentTombstoneRegistry.Clear();

        [TearDown]
        public void TearDown() => PersistentTombstoneRegistry.Clear();

        [Test]
        public void Capture_UsesStableIdAndParticipantPayload()
        {
            GameObject gameObject = new GameObject("Persistent");
            PersistentIdentity identity = gameObject.AddComponent<PersistentIdentity>();
            identity.AssignSceneId("object-a");
            TestPersistentParticipant participant = gameObject.AddComponent<TestPersistentParticipant>();
            participant.value = 42;

            var captured = PersistentWorldState.Capture(new[] { identity });

            Assert.That(captured.Single().objectId, Is.EqualTo("object-a"));
            Assert.That(captured.Single().components.Single().participantKey, Is.EqualTo("test"));
            Assert.That(captured.Single().components.Single().payload, Is.EqualTo("42"));
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void Restore_AppliesParticipantPayload()
        {
            GameObject gameObject = new GameObject("Persistent");
            PersistentIdentity identity = gameObject.AddComponent<PersistentIdentity>();
            identity.AssignSceneId("object-a");
            TestPersistentParticipant participant = gameObject.AddComponent<TestPersistentParticipant>();
            var data = new PersistentObjectSaveData { objectId = "object-a" };
            data.components.Add(new PersistentComponentSaveData { participantKey = "test", version = 2, payload = "17" });

            PersistentRestoreReport report = PersistentWorldState.Restore(new[] { identity }, new[] { data }, new string[0]);

            Assert.That(participant.value, Is.EqualTo(17));
            Assert.That(report.ObjectsRestored, Is.EqualTo(1));
            Assert.That(report.ComponentsRestored, Is.EqualTo(1));
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void Restore_TombstoneDisablesSceneObject()
        {
            GameObject gameObject = new GameObject("Persistent");
            PersistentIdentity identity = gameObject.AddComponent<PersistentIdentity>();
            identity.AssignSceneId("destroyed-a");

            PersistentRestoreReport report = PersistentWorldState.Restore(new[] { identity }, null, new[] { "destroyed-a" });

            Assert.That(gameObject.activeSelf, Is.False);
            Assert.That(report.DestroyedObjectsApplied, Is.EqualTo(1));
            Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void Migrator_VersionTwoInitialisesWorldCollections()
        {
            var data = new SaveGameData { schemaVersion = 2, worldObjects = null, destroyedObjectIds = null };
            SaveGameData migrated = SaveGameMigrator.Migrate(data);
            Assert.That(migrated.schemaVersion, Is.EqualTo(3));
            Assert.That(migrated.worldObjects, Is.Not.Null);
            Assert.That(migrated.destroyedObjectIds, Is.Not.Null);
        }

        [Test]
        public void TombstoneRegistry_EliminatesDuplicates()
        {
            Assert.That(PersistentTombstoneRegistry.MarkDestroyed("a"), Is.True);
            Assert.That(PersistentTombstoneRegistry.MarkDestroyed("a"), Is.False);
            Assert.That(PersistentTombstoneRegistry.Current.Count, Is.EqualTo(1));
        }
    }
}