using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using ZoneUA.Persistence;

namespace ZoneUA.Combat.Tests
{
    public sealed class SaveGamePersistenceTests
    {
        private string tempDirectory;

        [SetUp]
        public void SetUp()
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "ZoneUA_SaveTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDirectory)) Directory.Delete(tempDirectory, true);
        }

        [Test]
        public void Migrate_VersionOne_UpgradesAndClampsHealth()
        {
            var data = new SaveGameData
            {
                schemaVersion = 1,
                player = new PlayerSaveData { currentHealth = 150, maximumHealth = 0 }
            };

            SaveGameData migrated = SaveGameMigrator.Migrate(data);

            Assert.That(migrated.schemaVersion, Is.EqualTo(SaveGameData.CurrentSchemaVersion));
            Assert.That(migrated.player.maximumHealth, Is.EqualTo(150));
            Assert.That(migrated.player.currentHealth, Is.EqualTo(150));
        }

        [Test]
        public void Migrate_FutureVersion_Throws()
        {
            var data = new SaveGameData { schemaVersion = SaveGameData.CurrentSchemaVersion + 1 };
            Assert.Throws<InvalidOperationException>(() => SaveGameMigrator.Migrate(data));
        }

        [Test]
        public void Store_SaveAndLoad_RoundTripsSnapshot()
        {
            var store = new SaveSlotStore(tempDirectory);
            var data = new SaveGameData
            {
                activeScene = "Production",
                worldSeed = 42,
                player = new PlayerSaveData
                {
                    position = new Vector3(3f, 4f, 0f),
                    currentHealth = 73,
                    maximumHealth = 100,
                    activeWeaponIndex = 1
                }
            };
            data.Stamp("slot-a", DateTime.UtcNow);

            store.Save("slot-a", data);
            bool loaded = store.TryLoad("slot-a", out SaveGameData restored);

            Assert.That(loaded, Is.True);
            Assert.That(restored.activeScene, Is.EqualTo("Production"));
            Assert.That(restored.worldSeed, Is.EqualTo(42));
            Assert.That(restored.player.position, Is.EqualTo(new Vector3(3f, 4f, 0f)));
            Assert.That(restored.player.currentHealth, Is.EqualTo(73));
            Assert.That(restored.player.activeWeaponIndex, Is.EqualTo(1));
        }

        [Test]
        public void Store_CorruptedPrimary_FallsBackToBackup()
        {
            var store = new SaveSlotStore(tempDirectory);
            var first = new SaveGameData { worldSeed = 10 };
            first.Stamp("slot", DateTime.UtcNow);
            store.Save("slot", first);

            var second = new SaveGameData { worldSeed = 20 };
            second.Stamp("slot", DateTime.UtcNow);
            store.Save("slot", second);
            File.WriteAllText(store.GetSlotPath("slot"), "corrupted");

            Assert.That(store.TryLoad("slot", out SaveGameData restored), Is.True);
            Assert.That(restored.worldSeed, Is.EqualTo(10));
        }

        [Test]
        public void Store_Delete_RemovesPrimaryAndBackup()
        {
            var store = new SaveSlotStore(tempDirectory);
            var data = new SaveGameData();
            data.Stamp("slot", DateTime.UtcNow);
            store.Save("slot", data);
            store.Save("slot", data);

            store.Delete("slot");

            Assert.That(store.Exists("slot"), Is.False);
        }
    }
}
