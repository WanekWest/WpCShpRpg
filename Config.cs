using System.Text.Json;

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
        public uint g_hCVExpDamage { get; private set; }
        public uint g_hCVExpKill { get; private set; }
        public uint g_hCVExpKillBonus { get; private set; }
        public uint g_hCVExpKillMax { get; private set; }

        public uint g_hCVExpTeamwin { get; private set; }

        public uint g_hCVLastExperienceCount { get; private set; }

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
        public CShpRpgDatabaseConfig CShpRpgDatabase { get; set; }
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
                Console.WriteLine("Ошибка: Невозможно найти путь к модулю!");
                return false;
            }

            string configPath = Path.Combine(ParentDirectory, "configs/wpcshprpg.cfg");
            if (!File.Exists(configPath))
            {
                Console.WriteLine("Ошибка: Не удалось найти файл по пути configs/wpcshprpg.cfg!");
                return false;
                // TODO: Если нет файла конфигурации - создавать его.
            }

            try
            {
                Dictionary<string, string> ConfigData = Config.ParseConfigFile(configPath);

                if (ConfigData.TryGetValue("csshprpg_enable", out string? g_csshprpg_enable))
                {
                    g_hCVEnable = bool.Parse(g_csshprpg_enable);
                }

                if (ConfigData.TryGetValue("csshprpg_ffa", out string? g_csshprpg_ffa))
                {
                    g_hCVFFA = bool.Parse(g_csshprpg_ffa);
                }

                if (ConfigData.TryGetValue("csshprpg_bot_enable", out string? g_csshprpg_bot_enable))
                {
                    g_hCVBotEnable = bool.Parse(g_csshprpg_bot_enable);
                }

                if (ConfigData.TryGetValue("csshprpg_bot_save_stats", out string? g_csshprpg_bot_save_stats))
                {
                    g_hCVBotSaveStats = bool.Parse(g_csshprpg_bot_save_stats);
                }

                if (ConfigData.TryGetValue("csshprpg_bot_need_human", out string? g_csshprpg_bot_need_human))
                {
                    g_hCVBotNeedHuman = bool.Parse(g_csshprpg_bot_need_human);
                }

                if (ConfigData.TryGetValue("csshprpg_need_enemies", out string? g_csshprpg_need_enemies))
                {
                    g_hCVNeedEnemies = bool.Parse(g_csshprpg_need_enemies);
                }

                if (ConfigData.TryGetValue("csshprpg_enemies_not_afk", out string? g_csshprpg_enemies_not_afk))
                {
                    g_hCVEnemiesNotAFK = bool.Parse(g_csshprpg_enemies_not_afk);
                }

                if (ConfigData.TryGetValue("csshprpg_debug", out string? g_csshprpg_debug))
                {
                    g_hCVDebug = bool.Parse(g_csshprpg_debug);
                }

                if (ConfigData.TryGetValue("csshprpg_save_data", out string? g_csshprpg_save_data))
                {
                    g_hCVSaveData = bool.Parse(g_csshprpg_save_data);
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
                    g_hCVAllowSelfReset = bool.Parse(g_csshprpg_allow_selfreset);
                }

                if (ConfigData.TryGetValue("csshprpg_bot_maxlevel", out string? g_csshprpg_bot_maxlevel))
                {
                    g_hCVBotMaxlevel = uint.Parse(g_csshprpg_bot_maxlevel);
                }

                if (ConfigData.TryGetValue("csshprpg_bot_maxlevel_reset", out string? g_csshprpg_bot_maxlevel_reset))
                {
                    g_hCVBotMaxlevelReset = bool.Parse(g_csshprpg_bot_maxlevel_reset);
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
                    g_hCVBotKillPlayer = bool.Parse(g_csshprpg_bot_kill_player);
                }

                if (ConfigData.TryGetValue("csshprpg_player_kill_bot", out string? g_csshprpg_player_kill_bot))
                {
                    g_hCVPlayerKillBot = bool.Parse(g_csshprpg_player_kill_bot);
                }

                if (ConfigData.TryGetValue("csshprpg_bot_kill_bot", out string? g_csshprpg_bot_kill_bot))
                {
                    g_hCVBotKillBot = bool.Parse(g_csshprpg_bot_kill_bot);
                }

                if (ConfigData.TryGetValue("csshprpg_announce_newlvl", out string? g_csshprpg_announce_newlvl))
                {
                    g_hCVAnnounceNewLvl = bool.Parse(g_csshprpg_announce_newlvl);
                }

                if (ConfigData.TryGetValue("csshprpg_afk_time", out string? g_csshprpg_afk_time))
                {
                    g_hCVAFKTime = uint.Parse(g_csshprpg_afk_time);
                }

                if (ConfigData.TryGetValue("csshprpg_spawn_protect_noxp", out string? g_csshprpg_spawn_protect_noxp))
                {
                    g_hCVSpawnProtect = bool.Parse(g_csshprpg_spawn_protect_noxp);
                }

                if (ConfigData.TryGetValue("csshprpg_exp_notice", out string? g_csshprpg_exp_notice))
                {
                    g_hCVExpNotice = bool.Parse(g_csshprpg_exp_notice);
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
                    g_hCVExpDamage = uint.Parse(g_csshprpg_exp_damage);
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
                    g_hCVExpTeamwin = uint.Parse(g_csshprpg_exp_teamwin);
                }

                if (ConfigData.TryGetValue("csshprpg_lastexperience_count", out string? g_csshprpg_lastexperience_count))
                {
                    g_hCVLastExperienceCount = uint.Parse(g_csshprpg_lastexperience_count);
                }

                if (ConfigData.TryGetValue("csshprpg_level_start", out string? g_csshprpg_level_start))
                {
                    g_hCVLevelStart = uint.Parse(g_csshprpg_level_start);
                }

                if (ConfigData.TryGetValue("csshprpg_level_start_give_credits", out string? g_csshprpg_level_start_give_credits))
                {
                    g_hCVLevelStartGiveCredits = bool.Parse(g_csshprpg_level_start_give_credits);
                }

                if (ConfigData.TryGetValue("csshprpg_upgrade_start_levels_free", out string? g_csshprpg_upgrade_start_levels_free))
                {
                    g_hCVUpgradeStartLevelsFree = bool.Parse(g_csshprpg_upgrade_start_levels_free);
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
                    g_hCVAllowSellDisabled = bool.Parse(g_csshprpg_allow_sell_disabled_upgrade);
                }

                if (ConfigData.TryGetValue("csshprpg_ignore_level_barrier", out string? g_csshprpg_ignore_level_barrier))
                {
                    g_hCVIgnoreLevelBarrier = bool.Parse(g_csshprpg_ignore_level_barrier);
                }

                if (ConfigData.TryGetValue("csshprpg_allow_present_upgrade_usage", out string? g_csshprpg_allow_present_upgrade_usage))
                {
                    g_hCVAllowPresentUpgradeUsage = bool.Parse(g_csshprpg_allow_present_upgrade_usage);
                }

                if (ConfigData.TryGetValue("csshprpg_disable_level_selection", out string? g_csshprpg_disable_level_selection))
                {
                    g_hCVDisableLevelSelection = bool.Parse(g_csshprpg_disable_level_selection);
                }

                if (ConfigData.TryGetValue("csshprpg_show_maxlevel_in_menu", out string? g_csshprpg_show_maxlevel_in_menu))
                {
                    g_hCVShowMaxLevelInMenu = bool.Parse(g_csshprpg_show_maxlevel_in_menu);
                }

                if (ConfigData.TryGetValue("csshprpg_show_upgrades_teamlock", out string? g_csshprpg_show_upgrades_teamlock))
                {
                    g_hCVShowUpgradesOfOtherTeam = bool.Parse(g_csshprpg_show_upgrades_teamlock);
                }

                if (ConfigData.TryGetValue("csshprpg_buy_upgrades_teamlock", out string? g_csshprpg_buy_upgrades_teamlock))
                {
                    g_hCVBuyUpgradesOfOtherTeam = bool.Parse(g_csshprpg_buy_upgrades_teamlock);
                }

                if (ConfigData.TryGetValue("csshprpg_show_teamlock_notice_own_team", out string? g_csshprpg_show_teamlock_notice_own_team))
                {
                    g_hCVShowTeamlockNoticeOwnTeam = bool.Parse(g_csshprpg_show_teamlock_notice_own_team);
                }

                if (ConfigData.TryGetValue("csshprpg_show_upgrade_purchase_in_chat", out string? g_csshprpg_show_upgrade_purchase_in_chat))
                {
                    g_hCVShowUpgradePurchase = bool.Parse(g_csshprpg_show_upgrade_purchase_in_chat);
                }

                if (ConfigData.TryGetValue("csshprpg_show_menu_on_levelup", out string? g_csshprpg_show_menu_on_levelup))
                {
                    g_hCVShowMenuOnLevelDefault = bool.Parse(g_csshprpg_show_menu_on_levelup);
                }

                if (ConfigData.TryGetValue("csshprpg_fade_screen_on_levelup", out string? g_csshprpg_fade_screen_on_levelup))
                {
                    g_hCVFadeOnLevelDefault = bool.Parse(g_csshprpg_fade_screen_on_levelup);
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
                            Console.WriteLine($"Invalid number: {colorParts[i]}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
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

            var configPath = Path.Combine(parentDirectory.FullName, "configs/mysql.json");

            if (!File.Exists(configPath))
            {
                Console.WriteLine("Не удалось найти базу данных!");
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
    }
}
