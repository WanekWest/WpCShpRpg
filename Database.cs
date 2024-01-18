using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MySqlConnector;
using static WpCShpRpg.Upgrades;

namespace WpCShpRpg
{
    public class Database
    {
        string wpcshprpg_DB = "cshprpg";
        string TBL_PLAYERS = "players";
        string TBL_PLAYERUPGRADES = "player_upgrades";
        string TBL_UPGRADES = "upgrades";
        string TBL_SETTINGS = "settings";
        string sExtraOptions = " ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";

        string ConnectionString;

        private static Config config;
        private static PlayerData playerData;
        private static Upgrades upgrades;
        private static Menu menu;

        public Database(string ModulePath, Config cfg)
        {
            SetConfig(cfg);
            ConnectionString = GetConnectionString(ModulePath);
        }

        public void SetUpgrades(Upgrades upgrClass)
        {

            upgrades = upgrClass;
        }

        public void SetConfig(Config cfg)
        {
            config = cfg;
        }

        public void SetMenu(Menu mn)
        {
            menu = mn;
        }

        public void SetPlayerData(PlayerData pData)
        {
            playerData = pData;
        }

        private string GetConnectionString(string ModulePath)
        {
            var Configuration = config.LoadDatabaseConfig(ModulePath);

            if (Configuration.CShpRpgDatabase == null)
            {
                throw new Exception("Объект 'Database' не найден в конфигурационном файле.");
            }

            var dbConfig = Configuration.CShpRpgDatabase;

            return $"Server={dbConfig.Host};Database={dbConfig.Name};User ID={dbConfig.User};Password={dbConfig.Password};";
        }

        // Создание таблиц.
        public void InitDatabase()
        {
            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();

                string sQuery = $"CREATE TABLE IF NOT EXISTS {TBL_PLAYERS} (player_id INTEGER PRIMARY KEY AUTO_INCREMENT, name VARCHAR(64) NOT NULL DEFAULT ' ', steamid INTEGER DEFAULT NULL UNIQUE, level INTEGER DEFAULT 1, experience INTEGER DEFAULT 0, credits INTEGER DEFAULT 0, showmenu INTEGER DEFAULT 1, fadescreen INTEGER DEFAULT 1, lastseen INTEGER DEFAULT 0, lastreset INTEGER DEFAULT 0) {sExtraOptions}";
                MySqlCommand command = new MySqlCommand(sQuery, connection);
                command.ExecuteNonQuery();

                // Create the upgrades table.
                sQuery = $"CREATE TABLE IF NOT EXISTS {TBL_UPGRADES} (upgrade_id INTEGER PRIMARY KEY AUTO_INCREMENT, shortname VARCHAR(32) UNIQUE NOT NULL, date_added INTEGER){sExtraOptions}";
                command = new MySqlCommand(sQuery, connection);
                command.ExecuteNonQuery();

                // Create the player -> upgrades table.
                sQuery = $"CREATE TABLE IF NOT EXISTS {TBL_PLAYERUPGRADES} (player_id INTEGER, upgrade_id INTEGER, purchasedlevel INTEGER NOT NULL, selectedlevel INTEGER NOT NULL, enabled INTEGER DEFAULT 1, visuals INTEGER DEFAULT 1, sounds INTEGER DEFAULT 1, PRIMARY KEY(player_id, upgrade_id), FOREIGN KEY (player_id) REFERENCES {TBL_PLAYERS}(player_id) ON DELETE CASCADE, FOREIGN KEY (upgrade_id) REFERENCES {TBL_UPGRADES}(upgrade_id) ON DELETE CASCADE){sExtraOptions}";
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

            string sQuery;
            // Have players expire after x days and delete them from the database?
            if (g_hCVPlayerExpire > 0)
            {
                sQuery = $"SELECT player_id FROM {TBL_PLAYERS} WHERE (level <= 1 AND lastseen <= {Server.CurrentTime - 259200}) OR lastseen <= {Server.CurrentTime - (86400 * g_hCVPlayerExpire)}";
            }
            else
            {
                // Delete players who are Level 1 and haven't played for 3 days
                sQuery = $"SELECT player_id FROM {TBL_PLAYERS} WHERE (level <= 1 AND lastseen <= {Server.CurrentTime - 259200})";
            }

            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(sQuery, connection);
                command.ExecuteNonQuery();
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
                sQuery = $"DELETE FROM {TBL_PLAYERS}";
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
                        playerData.InitPlayer(i, false);

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
                    sQuery = $"UPDATE {TBL_PLAYERS} SET level = {iStartLevel}, experience = 0, credits = {iStartCredits}, lastreset = {Server.CurrentTime}";
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
                            playerData.SetPlayerLastReset(i, Server.CurrentTime);
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
            Upgrades.SaveUpgradeConfig(upgrade);

            using (MySqlConnection connection = new(ConnectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand($"SELECT upgrade_id FROM upgrades WHERE shortname = \"{upgrade.shortName}\";", connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
