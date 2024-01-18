using CounterStrikeSharp.API;
using System.Text.Json;
using System.Xml.Linq;

namespace WpCShpRpg
{
    public class Config
    {
        #region Основные параметры мода
        // Глобальные переменные мода.
        public bool g_hCVEnable { get; private set; }
        public bool g_hCVFFA { get; private set; }
        public bool g_hCVBotEnable { get; private set; }
        public bool g_hCVBotSaveStats { get; private set; }
        public bool g_hCVBotNeedHuman { get; private set; }
        public bool g_hCVNeedEnemies { get; private set; }
        public bool g_hCVEnemiesNotAFK { get; private set; }
        public bool g_hCVDebug { get; private set; }
        public bool g_hCVSaveData { get; private set; }

        public uint g_hCVSaveInterval { get; private set; }
        public uint g_hCVPlayerExpire { get; private set; }

        public bool g_hCVAllowSelfReset { get; private set; }

        public uint g_hCVBotMaxlevel { get; private set; }
        public bool g_hCVBotMaxlevelReset { get; private set; }
        public uint g_hCVPlayerMaxlevel { get; private set; }
        public uint g_hCVPlayerMaxlevelReset { get; private set; }

        public bool g_hCVBotKillPlayer { get; private set; }
        public bool g_hCVPlayerKillBot { get; private set; }
        public bool g_hCVBotKillBot { get; private set; }
        public bool g_hCVAnnounceNewLvl { get; private set; }

        public uint g_hCVAFKTime { get; private set; }

        public bool g_hCVSpawnProtect { get; private set; }
        public bool g_hCVExpNotice { get; private set; }

        public uint g_hCVExpMax { get; private set; }
        public uint g_hCVExpStart { get; private set; }
        public uint g_hCVExpInc { get; private set; }
        public float g_hCVExpDamage { get; private set; }
        public uint g_hCVExpKill { get; private set; }
        public uint g_hCVExpKillBonus { get; private set; }
        public uint g_hCVExpKillMax { get; private set; }

        public float g_hCVExpTeamwin { get; private set; }

        public int g_hCVLastExperienceCount { get; private set; }

        public uint g_hCVLevelStart { get; private set; }
        public bool g_hCVLevelStartGiveCredits { get; private set; }

        public bool g_hCVUpgradeStartLevelsFree { get; private set; }

        public uint g_hCVCreditsInc { get; private set; }
        public uint g_hCVCreditsStart { get; private set; }

        public float g_hCVSalePercent { get; private set; }
        public bool g_hCVAllowSellDisabled { get; private set; }
        public bool g_hCVIgnoreLevelBarrier { get; private set; }
        public bool g_hCVAllowPresentUpgradeUsage { get; private set; }
        public bool g_hCVDisableLevelSelection { get; private set; }
        public bool g_hCVShowMaxLevelInMenu { get; private set; }
        public bool g_hCVShowUpgradesOfOtherTeam { get; private set; }
        public bool g_hCVBuyUpgradesOfOtherTeam { get; private set; }
        public bool g_hCVShowTeamlockNoticeOwnTeam { get; private set; }
        public bool g_hCVShowUpgradePurchase { get; private set; }
        public bool g_hCVShowMenuOnLevelDefault { get; private set; }
        public bool g_hCVFadeOnLevelDefault { get; private set; }

        public uint[] g_hCVFadeOnLevelColor = new uint[4];
        #endregion

        #region Переменные для базы данных
        public CShpRpgDatabaseConfig CShpRpgDatabase { get; set; } = null;
        #endregion

        #region Парсинг Основного конфига
        public static Dictionary<string, string> ParseConfigFile(string filePath)
        {
            var configData = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                {
                    continue; // Skip comments and empty lines
                }

                var parts = line.Split(new[] { ' ' }, 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"'); // Remove extra quotes around the value
                    configData[key] = value;
                }
            }
            return configData;
        }

        public bool LoadModCondiguration(string ModuleDirectory)
        {
            string? ParentDirectory = Directory.GetParent(ModuleDirectory)?.Parent?.FullName;
            if (string.IsNullOrEmpty(ParentDirectory))
            {
                Server.PrintToConsole("Ошибка: Невозможно найти путь к модулю!");
                return false;
            }

            string configPath = Path.Combine(ParentDirectory, "configs/wpcshprpg.cfg");
            if (!File.Exists(configPath))
            {
                Server.PrintToConsole("Ошибка: Не удалось найти файл по пути configs/wpcshprpg.cfg!");
                return false;
                // TODO: Если нет файла конфигурации - создавать его.
            }

            try
            {
                Dictionary<string, string> ConfigData = Config.ParseConfigFile(configPath);

                if (ConfigData.TryGetValue("csshprpg_enable", out string? g_csshprpg_enable))
                {
                    g_hCVEnable = g_csshprpg_enable == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_ffa", out string? g_csshprpg_ffa))
                {
                    g_hCVFFA = g_csshprpg_ffa == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_bot_enable", out string? g_csshprpg_bot_enable))
                {
                    g_hCVBotEnable = g_csshprpg_bot_enable == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_bot_save_stats", out string? g_csshprpg_bot_save_stats))
                {
                    g_hCVBotSaveStats = g_csshprpg_bot_save_stats == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_bot_need_human", out string? g_csshprpg_bot_need_human))
                {
                    g_hCVBotNeedHuman = g_csshprpg_bot_need_human == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_need_enemies", out string? g_csshprpg_need_enemies))
                {
                    g_hCVNeedEnemies = g_csshprpg_need_enemies == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_enemies_not_afk", out string? g_csshprpg_enemies_not_afk))
                {
                    g_hCVEnemiesNotAFK = g_csshprpg_enemies_not_afk == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_debug", out string? g_csshprpg_debug))
                {
                    g_hCVDebug = g_csshprpg_debug == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_save_data", out string? g_csshprpg_save_data))
                {
                    g_hCVSaveData = g_csshprpg_save_data == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_save_interval", out string? g_csshprpg_save_interval))
                {
                    g_hCVSaveInterval = uint.Parse(g_csshprpg_save_interval);
                }

                if (ConfigData.TryGetValue("csshprpg_player_expire", out string? g_csshprpg_player_expire))
                {
                    g_hCVPlayerExpire = uint.Parse(g_csshprpg_player_expire);
                }

                if (ConfigData.TryGetValue("csshprpg_allow_selfreset", out string? g_csshprpg_allow_selfreset))
                {
                    g_hCVAllowSelfReset = g_csshprpg_allow_selfreset == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_bot_maxlevel", out string? g_csshprpg_bot_maxlevel))
                {
                    g_hCVBotMaxlevel = uint.Parse(g_csshprpg_bot_maxlevel);
                }

                if (ConfigData.TryGetValue("csshprpg_bot_maxlevel_reset", out string? g_csshprpg_bot_maxlevel_reset))
                {
                    g_hCVBotMaxlevelReset = g_csshprpg_bot_maxlevel_reset == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_player_maxlevel", out string? g_csshprpg_player_maxlevel))
                {
                    g_hCVPlayerMaxlevel = uint.Parse(g_csshprpg_player_maxlevel);
                }

                if (ConfigData.TryGetValue("csshprpg_player_maxlevel_reset", out string? g_csshprpg_player_maxlevel_reset))
                {
                    g_hCVPlayerMaxlevelReset = uint.Parse(g_csshprpg_player_maxlevel_reset);
                }

                if (ConfigData.TryGetValue("csshprpg_bot_kill_player", out string? g_csshprpg_bot_kill_player))
                {
                    g_hCVBotKillPlayer = g_csshprpg_bot_kill_player == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_player_kill_bot", out string? g_csshprpg_player_kill_bot))
                {
                    g_hCVPlayerKillBot = g_csshprpg_player_kill_bot == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_bot_kill_bot", out string? g_csshprpg_bot_kill_bot))
                {
                    g_hCVBotKillBot = g_csshprpg_bot_kill_bot == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_announce_newlvl", out string? g_csshprpg_announce_newlvl))
                {
                    g_hCVAnnounceNewLvl = g_csshprpg_announce_newlvl == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_afk_time", out string? g_csshprpg_afk_time))
                {
                    g_hCVAFKTime = uint.Parse(g_csshprpg_afk_time);
                }

                if (ConfigData.TryGetValue("csshprpg_spawn_protect_noxp", out string? g_csshprpg_spawn_protect_noxp))
                {
                    g_hCVSpawnProtect = g_csshprpg_spawn_protect_noxp == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_exp_notice", out string? g_csshprpg_exp_notice))
                {
                    g_hCVExpNotice = g_csshprpg_exp_notice == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_exp_max", out string? g_csshprpg_exp_max))
                {
                    g_hCVExpMax = uint.Parse(g_csshprpg_exp_max);
                }

                if (ConfigData.TryGetValue("csshprpg_exp_start", out string? g_csshprpg_exp_start))
                {
                    g_hCVExpStart = uint.Parse(g_csshprpg_exp_start);
                }

                if (ConfigData.TryGetValue("csshprpg_exp_inc", out string? g_csshprpg_exp_inc))
                {
                    g_hCVExpInc = uint.Parse(g_csshprpg_exp_inc);
                }

                if (ConfigData.TryGetValue("csshprpg_exp_damage", out string? g_csshprpg_exp_damage))
                {
                    g_hCVExpDamage = float.Parse(g_csshprpg_exp_damage);
                }

                if (ConfigData.TryGetValue("csshprpg_exp_kill", out string? g_csshprpg_exp_kill))
                {
                    g_hCVExpKill = uint.Parse(g_csshprpg_exp_kill);
                }

                if (ConfigData.TryGetValue("csshprpg_exp_kill_bonus", out string? g_csshprpg_exp_kill_bonus))
                {
                    g_hCVExpKillBonus = uint.Parse(g_csshprpg_exp_kill_bonus);
                }

                if (ConfigData.TryGetValue("csshprpg_exp_kill_max", out string? g_csshprpg_exp_kill_max))
                {
                    g_hCVExpKillMax = uint.Parse(g_csshprpg_exp_kill_max);
                }

                if (ConfigData.TryGetValue("csshprpg_exp_teamwin", out string? g_csshprpg_exp_teamwin))
                {
                    g_hCVExpTeamwin = float.Parse(g_csshprpg_exp_teamwin);
                }

                if (ConfigData.TryGetValue("csshprpg_lastexperience_count", out string? g_csshprpg_lastexperience_count))
                {
                    g_hCVLastExperienceCount = int.Parse(g_csshprpg_lastexperience_count);
                }

                if (ConfigData.TryGetValue("csshprpg_level_start", out string? g_csshprpg_level_start))
                {
                    g_hCVLevelStart = uint.Parse(g_csshprpg_level_start);
                }

                if (ConfigData.TryGetValue("csshprpg_level_start_give_credits", out string? g_csshprpg_level_start_give_credits))
                {
                    g_hCVLevelStartGiveCredits = g_csshprpg_level_start_give_credits == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_upgrade_start_levels_free", out string? g_csshprpg_upgrade_start_levels_free))
                {
                    g_hCVUpgradeStartLevelsFree = g_csshprpg_upgrade_start_levels_free == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_credits_inc", out string? g_csshprpg_credits_inc))
                {
                    g_hCVCreditsInc = uint.Parse(g_csshprpg_credits_inc);
                }

                if (ConfigData.TryGetValue("csshprpg_credits_start", out string? g_csshprpg_credits_start))
                {
                    g_hCVCreditsStart = uint.Parse(g_csshprpg_credits_start);
                }

                if (ConfigData.TryGetValue("csshprpg_sale_percent", out string? g_csshprpg_sale_percent))
                {
                    g_hCVSalePercent = float.Parse(g_csshprpg_sale_percent);
                }

                if (ConfigData.TryGetValue("csshprpg_allow_sell_disabled_upgrade", out string? g_csshprpg_allow_sell_disabled_upgrade))
                {
                    g_hCVAllowSellDisabled = g_csshprpg_allow_sell_disabled_upgrade == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_ignore_level_barrier", out string? g_csshprpg_ignore_level_barrier))
                {
                    g_hCVIgnoreLevelBarrier = g_csshprpg_ignore_level_barrier == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_allow_present_upgrade_usage", out string? g_csshprpg_allow_present_upgrade_usage))
                {
                    g_hCVAllowPresentUpgradeUsage = g_csshprpg_allow_present_upgrade_usage == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_disable_level_selection", out string? g_csshprpg_disable_level_selection))
                {
                    g_hCVDisableLevelSelection = g_csshprpg_disable_level_selection == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_show_maxlevel_in_menu", out string? g_csshprpg_show_maxlevel_in_menu))
                {
                    g_hCVShowMaxLevelInMenu = g_csshprpg_show_maxlevel_in_menu == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_show_upgrades_teamlock", out string? g_csshprpg_show_upgrades_teamlock))
                {
                    g_hCVShowUpgradesOfOtherTeam = g_csshprpg_show_upgrades_teamlock == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_buy_upgrades_teamlock", out string? g_csshprpg_buy_upgrades_teamlock))
                {
                    g_hCVBuyUpgradesOfOtherTeam = g_csshprpg_buy_upgrades_teamlock == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_show_teamlock_notice_own_team", out string? g_csshprpg_show_teamlock_notice_own_team))
                {
                    g_hCVShowTeamlockNoticeOwnTeam = g_csshprpg_show_teamlock_notice_own_team == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_show_upgrade_purchase_in_chat", out string? g_csshprpg_show_upgrade_purchase_in_chat))
                {
                    g_hCVShowUpgradePurchase = g_csshprpg_show_upgrade_purchase_in_chat == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_show_menu_on_levelup", out string? g_csshprpg_show_menu_on_levelup))
                {
                    g_hCVShowMenuOnLevelDefault = g_csshprpg_show_menu_on_levelup == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_fade_screen_on_levelup", out string? g_csshprpg_fade_screen_on_levelup))
                {
                    g_hCVFadeOnLevelDefault = g_csshprpg_fade_screen_on_levelup == "1";
                }

                if (ConfigData.TryGetValue("csshprpg_fade_screen_on_levelup_color", out string? g_csshprpg_fade_screen_on_levelup_color))
                {
                    string[] colorParts = g_csshprpg_fade_screen_on_levelup_color.Split(' ');
                    for (int i = 0; i < colorParts.Length; i++)
                    {
                        if (uint.TryParse(colorParts[i], out uint value))
                        {
                            g_hCVFadeOnLevelColor[i] = value;
                        }
                        else
                        {
                            Server.PrintToConsole($"Invalid number: {colorParts[i]}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Error loading configuration: {ex.Message}");
                return false;
            }

            return true;
        }
        #endregion

        #region Работа с базами
        private Config CreateDatabaseConfig(string configPath)
        {
            var config = new Config
            {
                CShpRpgDatabase = new CShpRpgDatabaseConfig
                {
                    Host = "",
                    Name = "",
                    User = "",
                    Password = "",
                }
            };

            File.WriteAllText(configPath,
                JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

            return config;
        }

        public Config LoadDatabaseConfig(string ModulePath)
        {
            var moduleDirectoryParent = Directory.GetParent(ModulePath);
            if (moduleDirectoryParent == null)
            {
                throw new InvalidOperationException("Не удалось найти родительский каталог модуля.");
            }

            var parentDirectory = moduleDirectoryParent.Parent;
            if (parentDirectory == null)
            {
                throw new InvalidOperationException("Не удалось найти родительский каталог родительского каталога модуля.");
            }

            var configPath = Path.Combine(parentDirectory.FullName, "configs/wpcshprpg_mysql.json");
            if (!File.Exists(configPath))
            {
                Server.PrintToConsole("Не удалось найти базу данных!");
                return CreateDatabaseConfig(ModulePath);
            }

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

            return config;
        }

        public class CShpRpgDatabaseConfig
        {
            public required string Host { get; init; }
            public required string Name { get; init; }
            public required string User { get; init; }
            public required string Password { get; init; }
        }
        #endregion

        public bool CreateSkillConfig(string ModuleDirectory, string ShortSkillName, string sName)
        {
            string? ParentDirectory = Directory.GetParent(ModuleDirectory)?.Parent?.FullName;
            if (string.IsNullOrEmpty(ParentDirectory))
            {
                Server.PrintToConsole("Ошибка: Невозможно найти путь к модулю!");
                return false;
            }

            string configPath = Path.Combine(ParentDirectory, $"configs/wpcshrpg/{ShortSkillName}.cfg");
            if (!File.Exists(configPath))
            {
                string[] content = new string[]
                {
                "// Enables (1) or disables (0) the " + sName + " upgrade.",
                "wpcshprpg_" + ShortSkillName + "_enable \"1\"",
                "// " + sName + " upgrade maximum level. This is the maximum level players can reach for this upgrade.",
                "wpcshprpg_" + ShortSkillName + "_maxlevel \"5\"",
                "// " + sName + " upgrade start level. The initial levels players get of this upgrade when they first join the server.",
                "wpcshprpg_" + ShortSkillName + "_startlevel \"0\"",
                "// " + sName + " upgrade start cost. The initial amount of credits the first level of this upgrade costs.",
                "wpcshprpg_" + ShortSkillName + "_cost \"100\"",
                "// " + sName + " upgrade cost increment for each level.",
                "wpcshprpg_" + ShortSkillName + "_icost \"20\"",
                "// Required admin flag to use this upgrade.",
                "wpcshprpg_" + ShortSkillName + "_adminflag \"\"",
                "// Allow bots to use the " + sName + " upgrade?",
                "wpcshprpg_" + ShortSkillName + "_allowbots \"1\"",
                "// Restrict access to the " + sName + " upgrade to a team?",
                "wpcshprpg_" + ShortSkillName + "_teamlock \"0\""
                };
                File.WriteAllLines(configPath, content);
            }

            return true;
        }

        public Dictionary<string, string> GetParamsFromConfig(string ModuleDirectory, string ShortSkillName)
        {
            string? ParentDirectory = Directory.GetParent(ModuleDirectory)?.Parent?.FullName;
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(ParentDirectory))
            {
                Server.PrintToConsole("Ошибка: Невозможно найти путь к модулю!");
                return parameters;
            }

            string configPath = Path.Combine(ParentDirectory, $"configs/wpcshrpg/{ShortSkillName}.cfg");
            if (!File.Exists(configPath))
            {
                Server.PrintToConsole($"Ошибка: Не удалось найти файл по пути {configPath}!");
                return parameters;
            }

            string[] lines = File.ReadAllLines(configPath);
            foreach (string line in lines)
            {
                if (!line.StartsWith("//") && !string.IsNullOrWhiteSpace(line))
                {
                    string[] parts = line.Split(new char[] { ' ' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim().Trim('\"');
                        parameters[key] = value;
                    }
                }
            }

            return parameters;
        }

        public bool AddParametrToSkillConfig(string ModuleDirectory, string ShortSkillName, string sName)
        {
            string? ParentDirectory = Directory.GetParent(ModuleDirectory)?.Parent?.FullName;
            if (string.IsNullOrEmpty(ParentDirectory))
            {
                Server.PrintToConsole("Ошибка: Невозможно найти путь к модулю!");
                return false;
            }

            string configPath = Path.Combine(ParentDirectory, $"configs/wpcshrpg/{ShortSkillName}.cfg");
            if (!File.Exists(configPath))
            {
                string[] content = new string[]
                {
                "// Enables (1) or disables (0) the " + sName + " upgrade.",
                "wpcshprpg_" + ShortSkillName + "_enable \"1\"",
                "// " + sName + " upgrade maximum level. This is the maximum level players can reach for this upgrade.",
                "wpcshprpg_" + ShortSkillName + "_maxlevel \"5\"",
                "// " + sName + " upgrade start level. The initial levels players get of this upgrade when they first join the server.",
                "wpcshprpg_" + ShortSkillName + "_startlevel \"0\"",
                "// " + sName + " upgrade start cost. The initial amount of credits the first level of this upgrade costs.",
                "wpcshprpg_" + ShortSkillName + "_cost \"100\"",
                "// " + sName + " upgrade cost increment for each level.",
                "wpcshprpg_" + ShortSkillName + "_icost \"20\"",
                "// Required admin flag to use this upgrade.",
                "wpcshprpg_" + ShortSkillName + "_adminflag \"\"",
                "// Allow bots to use the " + sName + " upgrade?",
                "wpcshprpg_" + ShortSkillName + "_allowbots \"1\"",
                "// Restrict access to the " + sName + " upgrade to a team?",
                "wpcshprpg_" + ShortSkillName + "_teamlock \"0\""
                };
                File.WriteAllLines(configPath, content);
            }

            return true;
        }
    }
}
