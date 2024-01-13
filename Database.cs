using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using MySqlConnector;

namespace WpCShpRpg
{
    public class Database
    {
        string SMRPG_DB = "cshprpg";
        string TBL_PLAYERS = "players";
        string TBL_PLAYERUPGRADES = "player_upgrades";
        string TBL_UPGRADES = "upgrades";
        string TBL_SETTINGS = "settings";
        string sExtraOptions = " ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";

        string ConnectionString;

        Config config = null;
        PlayerData playerData = null;
        Upgrades upgrades = null;
        Menu menu = null;

        public Database(string ModulePath, Config config, PlayerData playerData, Upgrades upgrades, Menu menu)
        {
            ConnectionString = GetConnectionString(ModulePath);
            this.config = config;
            this.playerData = playerData;
            this.upgrades = upgrades;
            this.menu = menu;
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

                string sQuery = $"CREATE TABLE IF NOT EXISTS {TBL_PLAYERS} (player_id INTEGER PRIMARY KEY AUTO_INCREMENT, name VARCHAR(64) NOT NULL DEFAULT ' ', steamid INTEGER DEFAULT NULL UNIQUE, level INTEGER DEFAULT 1, experience INTEGER DEFAULT 0, credits INTEGER DEFAULT 0, showmenu INTEGER DEFAULT 1, fadescreen INTEGER DEFAULT 1, lastseen INTEGER DEFAULT 0, lastreset INTEGER DEFAULT 0) {sExtraOptions}");
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

        public bool ResetAllPlayers(bool g_hCVSaveData, string sReason, bool bHardReset)
        {
            // Don't touch the database, if we don't want to save any data.
            if (!g_hCVSaveData)
                return false;

            PlayerData playerData = new PlayerData();

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
                        playerData.RemovePlayer(i, true, config.g_hCVShowMenuOnLevelDefault, config.g_hCVFadeOnLevelDefault);
                        playerData.InitPlayer(i, false);

                        if (Player.IsValid)
                            playerData.InsertPlayer(i, config.g_hCVEnable, g_hCVSaveData, config.g_hCVBotSaveStats);
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
    }
}
