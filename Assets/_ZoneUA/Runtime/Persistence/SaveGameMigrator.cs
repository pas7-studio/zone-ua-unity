using System;

namespace ZoneUA.Persistence
{
    public static class SaveGameMigrator
    {
        public static SaveGameData Migrate(SaveGameData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.schemaVersion <= 0) data.schemaVersion = 1;
            if (data.schemaVersion > SaveGameData.CurrentSchemaVersion)
                throw new InvalidOperationException($"Save schema {data.schemaVersion} is newer than supported schema {SaveGameData.CurrentSchemaVersion}.");

            if (data.schemaVersion == 1)
            {
                data.player ??= new PlayerSaveData();
                if (data.player.maximumHealth <= 0) data.player.maximumHealth = Math.Max(1, data.player.currentHealth);
                data.schemaVersion = 2;
            }

            data.player ??= new PlayerSaveData();
            data.player.maximumHealth = Math.Max(1, data.player.maximumHealth);
            data.player.currentHealth = Math.Clamp(data.player.currentHealth, 0, data.player.maximumHealth);
            return data;
        }
    }
}
