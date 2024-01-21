using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using MySqlConnector;
using static WpCShpRpg.Core.Additions.PlayerData;
using static WpCShpRpg.Core.Additions.Upgrades;

namespace WpCShpRpg.Core.Additions
{
    public class Database
    {
        string wpcshprpg_DB = "cshprpg";
        string TBL_PLAYERS = "players";
        string TBL_PLAYERUPGRADES = "player_upgrades";
        string TBL_UPGRADES = "upgrades";
        string TBL_SETTINGS = "settings";
        string sExtraOptions = " ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";

        private static ConfiguraionFiles? config;
        private static PlayerData? playerData;
        private static Upgrades? upgrades;
        private static Menu? menu;

        private string ConnectionString = string.Empty;

        public Database(ConfiguraionFiles cfg, string ConnectionString)
        {
            config = cfg;

            this.ConnectionString = ConnectionString;
            if (ConnectionString == string.Empty)
            {
                Console.WriteLine("Ошибка в файле с базой данных!");
            }
        }

        public void SetUpgrades(Upgrades upgrClass)
        {
            upgrades = upgrClass;
        }

        public void SetMenu(Menu mn)
        {
            menu = mn;
        }

        public void SetPlayerData(PlayerData pData)
        {
            playerData = pData;
        }

        // Создание таблиц.
        public void InitDatabase()
        {
            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();

                string sQuery = $"CREATE TABLE IF NOT EXISTS players (player_id INTEGER PRIMARY KEY AUTO_INCREMENT, name VARCHAR(64) NOT NULL DEFAULT ' ', steamid INTEGER DEFAULT NULL UNIQUE, level INTEGER DEFAULT 1, experience INTEGER DEFAULT 0, credits INTEGER DEFAULT 0, showmenu INTEGER DEFAULT 1, fadescreen INTEGER DEFAULT 1, lastseen INTEGER DEFAULT 0, lastreset INTEGER DEFAULT 0) {sExtraOptions}";
                MySqlCommand command = new MySqlCommand(sQuery, connection);
                command.ExecuteNonQuery();

                // Create the upgrades table.
                sQuery = $"CREATE TABLE IF NOT EXISTS {TBL_UPGRADES} (upgrade_id INTEGER PRIMARY KEY AUTO_INCREMENT, shortname VARCHAR(32) UNIQUE NOT NULL, date_added INTEGER){sExtraOptions}";
                command = new MySqlCommand(sQuery, connection);
                command.ExecuteNonQuery();

                // Create the player -> upgrades table.
                sQuery = $"CREATE TABLE IF NOT EXISTS {TBL_PLAYERUPGRADES} (player_id INTEGER, upgrade_id INTEGER, purchasedlevel INTEGER NOT NULL, selectedlevel INTEGER NOT NULL, enabled INTEGER DEFAULT 1, visuals INTEGER DEFAULT 1, sounds INTEGER DEFAULT 1, PRIMARY KEY(player_id, upgrade_id), FOREIGN KEY (player_id) REFERENCES players (player_id) ON DELETE CASCADE, FOREIGN KEY (upgrade_id) REFERENCES {TBL_UPGRADES}(upgrade_id) ON DELETE CASCADE){sExtraOptions}";
                command = new MySqlCommand(sQuery, connection);
                command.ExecuteNonQuery();

                // Create the settings table.
                sQuery = $"CREATE TABLE IF NOT EXISTS {TBL_SETTINGS} (setting VARCHAR(64) PRIMARY KEY NOT NULL, value VARCHAR(256) NOT NULL){sExtraOptions}";
                command = new MySqlCommand(sQuery, connection);
                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        // Очистка от АФКшников.
        public void DatabaseMaid(bool g_hCVSaveData, uint g_hCVPlayerExpire)
        {
            // Don't touch the database, if we don't want to save any data.
            if (!g_hCVSaveData)
                return;

            DateTime currentTime = DateTime.Now;
            // Время "3 дня назад" в формате Unix Timestamp
            DateTime threeDaysAgo = currentTime.AddSeconds(-259200);
            long threeDaysAgoUnix = ((DateTimeOffset)threeDaysAgo).ToUnixTimeSeconds();

            string sQuery;
            if (g_hCVPlayerExpire > 0)
            {
                DateTime expireTime = currentTime.AddSeconds(-86400 * g_hCVPlayerExpire);
                long expireTimeUnix = ((DateTimeOffset)expireTime).ToUnixTimeSeconds();
                sQuery = $"SELECT player_id FROM players WHERE (level <= 1 AND lastseen <= {threeDaysAgoUnix}) OR lastseen <= {expireTimeUnix}";
            }
            else
            {
                // Удаляем игроков, которые находятся на уровне 1 и не играли в течение 3 дней
                sQuery = $"SELECT player_id FROM players WHERE (level <= 1 AND lastseen <= {threeDaysAgoUnix})";
            }

            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                MySqlCommand selectCommand = new MySqlCommand(sQuery, connection);
                List<int> playerIds = new List<int>();

                try
                {
                    connection.Open();
                    using (MySqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            playerIds.Add(reader.GetInt32(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Обработка возможных исключений при выполнении SELECT
                    Server.PrintToConsole($"Ошибка при выборе игроков для удаления: {ex.Message}");
                }

                // После получения списка идентификаторов игроков, удаляем их
                foreach (var playerId in playerIds)
                {
                    string deleteQuery = $"DELETE FROM players WHERE player_id = {playerId}";
                    MySqlCommand deleteCommand = new MySqlCommand(deleteQuery, connection);

                    try
                    {
                        deleteCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        // Обработка возможных исключений при выполнении DELETE
                        Server.PrintToConsole($"Ошибка при удалении игрока с ID {playerId}: {ex.Message}");
                    }
                }

                connection.Close();
            }
        }

        public bool ResetAllPlayers(string sReason, bool bHardReset)
        {
            // Don't touch the database, if we don't want to save any data.
            if (!config.g_hCVSaveData)
                return false;

            string sQuery;
            // Delete all player information?
            if (bHardReset)
            {
                sQuery = $"DELETE FROM players";
                using (MySqlConnection connection = new(ConnectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(sQuery, connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                // Reset all ingame players and read them into the database.
                for (int i = 1; i <= Server.MaxPlayers; i++)
                {
                    CCSPlayerController Player = Utilities.GetPlayerFromIndex(i);
                    if (Player != null && Player.IsValid && !Player.IsBot && Player.UserId > 0)
                    {
                        // Keep the original bot names intact, to avoid saving renamed bots.
                        playerData.RemovePlayer(i, config.g_hCVShowMenuOnLevelDefault, config.g_hCVFadeOnLevelDefault, true);
                        InitPlayer(i, false);

                        if (Player.IsValid)
                            playerData.InsertPlayer(i, config.g_hCVEnable, config.g_hCVSaveData, config.g_hCVBotSaveStats);
                    }
                }
            }
            // Keep the player settings
            else
            {
                uint iStartLevel = config.g_hCVLevelStart, iStartCredits = config.g_hCVCreditsStart;
                using (MySqlConnection connection = new(ConnectionString))
                {
                    connection.Open();
                    sQuery = $"UPDATE players SET level = {iStartLevel}, experience = 0, credits = {iStartCredits}, lastreset = {((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds()}";
                    MySqlCommand command = new MySqlCommand(sQuery, connection);
                    command.ExecuteNonQuery();

                    sQuery = $"UPDATE {TBL_PLAYERUPGRADES} SET purchasedlevel = 0, selectedlevel = 0, enabled = 1";
                    command = new MySqlCommand(sQuery, connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                }

                // Just reset all ingame players too
                for (int i = 1; i <= Server.MaxPlayers; i++)
                {
                    CCSPlayerController Player = Utilities.GetPlayerFromIndex(i);
                    if (Player != null && Player.IsValid && !Player.IsBot && Player.UserId > 0)
                    {
                        if (Player.IsValid)
                        {
                            playerData.ResetStats(i);
                            playerData.SetPlayerLastReset(i, ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds());
                        }
                    }
                }
            }

            return true;
        }

        public void SendQuery(string Query)
        {
            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(Query, connection);
                command = new MySqlCommand(Query, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public uint GetPlayerRank(CCSPlayerController? player)
        {
            uint CurrentPlayerRank = 0;
            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand($"SELECT COUNT(*) FROM players WHERE level > {playerData.GetClientLevel((int)player.UserId)} OR (level = {playerData.GetClientLevel((int)player.UserId)} AND experience > {playerData.GetClientExperience((int)player.UserId)})", connection);
                CurrentPlayerRank = (uint)command.ExecuteScalar();
                connection.Close();
            }

            return CurrentPlayerRank;
        }

        public uint GetAmountOfRanks()
        {
            uint AmountOfRanks = 0;
            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand($"SELECT COUNT(*) FROM players", connection);
                AmountOfRanks = (uint)command.ExecuteScalar();
                connection.Close();
            }
            return AmountOfRanks;
        }

        public void CheckUpgradeDatabaseEntry(InternalUpgradeInfo upgrade)
        {
            upgrade.databaseLoading = true;
            SaveUpgradeConfig(upgrade);

            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand($"SELECT upgrade_id FROM upgrades WHERE shortname = \"{upgrade.shortName}\";", connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void GetPlayerInfo(string sQuery, int client)
        {
            Server.PrintToConsole("GetPlayerInfo GetPlayerInfo GetPlayerInfo!");

            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(sQuery, connection);
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        g_iPlayerInfo[client].dbId = reader.GetInt32(0);
                        g_iPlayerInfo[client].level = reader.GetUInt32(1);
                        g_iPlayerInfo[client].experience = reader.GetUInt32(2);
                        g_iPlayerInfo[client].credits = reader.GetUInt32(3);
                        g_iPlayerInfo[client].lastReset = reader.GetInt32(4);
                        g_iPlayerInfo[client].lastSeen = reader.GetInt32(5);
                        g_iPlayerInfo[client].showMenuOnLevelup = reader.GetInt32(6) != 0;
                        g_iPlayerInfo[client].fadeOnLevelup = reader.GetInt32(7) != 0;
                    }
                }

                string query = $"SELECT upgrade_id, purchasedlevel, selectedlevel, enabled, visuals, sounds FROM player_upgrades WHERE player_id = @playerId";
                try
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            playerData.InsertPlayer(client, config.g_hCVEnable, config.g_hCVSaveData, config.g_hCVBotSaveStats);
                            return;
                        }

                        if (reader.Read())
                        {
                            g_iPlayerInfo[client].dataLoadedFromDB = true;

                            int upgradeId = reader.GetInt32(0);
                            InternalUpgradeInfo upgrade = upgrades.GetUpgradeByDatabaseId(upgradeId);
                            PlayerUpgradeInfo playerupgrade = GetPlayerUpgradeInfoByIndex(client, upgrade.index);

                            playerupgrade.purchasedlevel = reader.GetUInt32(1);
                            playerupgrade.selectedlevel = reader.GetUInt32(2);
                            playerupgrade.enabled = reader.GetBoolean(3);
                            playerupgrade.visuals = reader.GetBoolean(4);
                            playerupgrade.sounds = reader.GetBoolean(5);

                            playerData.SavePlayerUpgradeInfo(client, upgrade.index, playerupgrade);

                            upgrades.SetClientPurchasedUpgradeLevel(client, upgrade.index, reader.GetUInt32(1));

                            // Make sure the database is sane.. People WILL temper with it manually.
                            uint SelectedLevel = reader.GetUInt32(2);
                            if (SelectedLevel > upgrades.GetClientPurchasedUpgradeLevel(client, upgrade.index))
                                SelectedLevel = upgrades.GetClientPurchasedUpgradeLevel(client, upgrade.index);

                            upgrades.SetClientSelectedUpgradeLevel(client, upgrade.index, SelectedLevel);
                        }

                        playerData.CheckItemMaxLevels(client);
                    }
                }
                catch (Exception ex)
                {
                    Server.PrintToConsole($"Unable to load player data: {ex.Message}");
                }

                connection.Close();
            }
        }
    }
}
