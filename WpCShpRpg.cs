using CounterStrikeSharp.API.Core;

namespace WpCShpRpg
{
    public class WpCShpRpg : BasePlugin
    {
        // БИО.
        public override string ModuleName => "WpCShpRPG";
        public override string ModuleVersion => "0.0.1";

        // Глобальные переменные мода.
        bool g_hCVEnable;
        bool g_hCVFFA;
        bool g_hCVBotEnable;
        bool g_hCVBotSaveStats;
        bool g_hCVBotNeedHuman;
        bool g_hCVNeedEnemies;
        bool g_hCVEnemiesNotAFK;
        bool g_hCVDebug;
        bool g_hCVSaveData;

        uint g_hCVSaveInterval;
        uint g_hCVPlayerExpire;

        bool g_hCVAllowSelfReset;

        uint g_hCVBotMaxlevel;
        uint g_hCVBotMaxlevelReset;
        uint g_hCVPlayerMaxlevel;
        uint g_hCVPlayerMaxlevelReset;

        bool g_hCVBotKillPlayer;
        bool g_hCVPlayerKillBot;
        bool g_hCVBotKillBot;
        bool g_hCVAnnounceNewLvl;

        uint g_hCVAFKTime;

        bool g_hCVSpawnProtect;
        bool g_hCVExpNotice;

        uint g_hCVExpMax;
        uint g_hCVExpStart;
        uint g_hCVExpInc;
        uint g_hCVExpDamage;
        uint g_hCVExpKill;
        uint g_hCVExpKillBonus;
        uint g_hCVExpKillMax;

        bool g_hCVExpTeamwin;

        uint g_hCVLastExperienceCount;

        uint g_hCVLevelStart;
        uint g_hCVLevelStartGiveCredits;

        bool g_hCVUpgradeStartLevelsFree;

        uint g_hCVCreditsInc;
        uint g_hCVCreditsStart;

        float g_hCVSalePercent;
        bool g_hCVAllowSellDisabled;
        bool g_hCVIgnoreLevelBarrier;
        bool g_hCVAllowPresentUpgradeUsage;
        bool g_hCVDisableLevelSelection;
        bool g_hCVShowMaxLevelInMenu;
        bool g_hCVShowUpgradesOfOtherTeam;
        bool g_hCVBuyUpgradesOfOtherTeam;
        bool g_hCVShowTeamlockNoticeOwnTeam;
        bool g_hCVShowUpgradePurchase;
        bool g_hCVShowMenuOnLevelDefault;
        bool g_hCVFadeOnLevelDefault;

        uint[] g_hCVFadeOnLevelColor = new uint[4];

        public override void Load(bool hotReload)
        {
            string? ParentDirectory = Directory.GetParent(ModuleDirectory)?.Parent?.FullName;
            if (string.IsNullOrEmpty(ParentDirectory))
            {
                Console.WriteLine("Error: Unable to find the parent directory for the module.");
                return;
            }

            string configPath = Path.Combine(ParentDirectory, "configs/wpcshprpg.cfg");
            if (!File.Exists(configPath))
            {
                Console.WriteLine("Error: Unable to find the parent directory for the module.");
                return;
                // TODO: Если нет файла конфигурации - создавать его.
            }

            try
            {
                Dictionary<string, string> ConfigData = Config.ParseConfigFile(configPath);

                if (ConfigData.TryGetValue("smrpg_enable", out string? g_smrpg_enable))
                {
                    g_hCVEnable = bool.Parse(g_smrpg_enable);
                }

                if (ConfigData.TryGetValue("smrpg_ffa", out string? g_smrpg_ffa))
                {
                    g_hCVFFA = bool.Parse(g_smrpg_ffa);
                }

                if (ConfigData.TryGetValue("smrpg_bot_enable", out string? g_smrpg_bot_enable))
                {
                    g_hCVBotEnable = bool.Parse(g_smrpg_bot_enable);
                }

                if (ConfigData.TryGetValue("smrpg_bot_save_stats", out string? g_smrpg_bot_save_stats))
                {
                    g_hCVBotSaveStats = bool.Parse(g_smrpg_bot_save_stats);
                }

                if (ConfigData.TryGetValue("smrpg_bot_need_human", out string? g_smrpg_bot_need_human))
                {
                    g_hCVBotNeedHuman = bool.Parse(g_smrpg_bot_need_human);
                }

                if (ConfigData.TryGetValue("smrpg_need_enemies", out string? g_smrpg_need_enemies))
                {
                    g_hCVNeedEnemies = bool.Parse(g_smrpg_need_enemies);
                }

                if (ConfigData.TryGetValue("smrpg_enemies_not_afk", out string? g_smrpg_enemies_not_afk))
                {
                    g_hCVEnemiesNotAFK = bool.Parse(g_smrpg_enemies_not_afk);
                }

                if (ConfigData.TryGetValue("smrpg_debug", out string? g_smrpg_debug))
                {
                    g_hCVDebug = bool.Parse(g_smrpg_debug);
                }

                if (ConfigData.TryGetValue("smrpg_save_data", out string? g_smrpg_save_data))
                {
                    g_hCVSaveData = bool.Parse(g_smrpg_save_data);
                }

                if (ConfigData.TryGetValue("smrpg_save_interval", out string? g_smrpg_save_interval))
                {
                    g_hCVSaveInterval = uint.Parse(g_smrpg_save_interval);
                }

                if (ConfigData.TryGetValue("smrpg_player_expire", out string? g_smrpg_player_expire))
                {
                    g_hCVPlayerExpire = uint.Parse(g_smrpg_player_expire);
                }

                if (ConfigData.TryGetValue("smrpg_allow_selfreset", out string? g_smrpg_allow_selfreset))
                {
                    g_hCVAllowSelfReset = bool.Parse(g_smrpg_allow_selfreset);
                }

                if (ConfigData.TryGetValue("smrpg_bot_maxlevel", out string? g_smrpg_bot_maxlevel))
                {
                    g_hCVBotMaxlevel = uint.Parse(g_smrpg_bot_maxlevel);
                }

                if (ConfigData.TryGetValue("smrpg_bot_maxlevel_reset", out string? g_smrpg_bot_maxlevel_reset))
                {
                    g_hCVBotMaxlevelReset = uint.Parse(g_smrpg_bot_maxlevel_reset);
                }

                if (ConfigData.TryGetValue("smrpg_player_maxlevel", out string? g_smrpg_player_maxlevel))
                {
                    g_hCVPlayerMaxlevel = uint.Parse(g_smrpg_player_maxlevel);
                }

                if (ConfigData.TryGetValue("smrpg_player_maxlevel_reset", out string? g_smrpg_player_maxlevel_reset))
                {
                    g_hCVPlayerMaxlevelReset = uint.Parse(g_smrpg_player_maxlevel_reset);
                }

                if (ConfigData.TryGetValue("smrpg_bot_kill_player", out string? g_smrpg_bot_kill_player))
                {
                    g_hCVBotKillPlayer = bool.Parse(g_smrpg_bot_kill_player);
                }

                if (ConfigData.TryGetValue("smrpg_player_kill_bot", out string? g_smrpg_player_kill_bot))
                {
                    g_hCVPlayerKillBot = bool.Parse(g_smrpg_player_kill_bot);
                }

                if (ConfigData.TryGetValue("smrpg_bot_kill_bot", out string? g_smrpg_bot_kill_bot))
                {
                    g_hCVBotKillBot = bool.Parse(g_smrpg_bot_kill_bot);
                }

                if (ConfigData.TryGetValue("smrpg_announce_newlvl", out string? g_smrpg_announce_newlvl))
                {
                    g_hCVAnnounceNewLvl = bool.Parse(g_smrpg_announce_newlvl);
                }

                if (ConfigData.TryGetValue("smrpg_afk_time", out string? g_smrpg_afk_time))
                {
                    g_hCVAFKTime = uint.Parse(g_smrpg_afk_time);
                }

                if (ConfigData.TryGetValue("smrpg_spawn_protect_noxp", out string? g_smrpg_spawn_protect_noxp))
                {
                    g_hCVSpawnProtect = bool.Parse(g_smrpg_spawn_protect_noxp);
                }

                if (ConfigData.TryGetValue("smrpg_exp_notice", out string? g_smrpg_exp_notice))
                {
                    g_hCVExpNotice = bool.Parse(g_smrpg_exp_notice);
                }

                if (ConfigData.TryGetValue("smrpg_exp_max", out string? g_smrpg_exp_max))
                {
                    g_hCVExpMax = uint.Parse(g_smrpg_exp_max);
                }

                if (ConfigData.TryGetValue("smrpg_exp_start", out string? g_smrpg_exp_start))
                {
                    g_hCVExpStart = uint.Parse(g_smrpg_exp_start);
                }

                if (ConfigData.TryGetValue("smrpg_exp_inc", out string? g_smrpg_exp_inc))
                {
                    g_hCVExpInc = uint.Parse(g_smrpg_exp_inc);
                }

                if (ConfigData.TryGetValue("smrpg_exp_damage", out string? g_smrpg_exp_damage))
                {
                    g_hCVExpDamage = uint.Parse(g_smrpg_exp_damage);
                }

                if (ConfigData.TryGetValue("smrpg_exp_kill", out string? g_smrpg_exp_kill))
                {
                    g_hCVExpKill = uint.Parse(g_smrpg_exp_kill);
                }

                if (ConfigData.TryGetValue("smrpg_exp_kill_bonus", out string? g_smrpg_exp_kill_bonus))
                {
                    g_hCVExpKillBonus = uint.Parse(g_smrpg_exp_kill_bonus);
                }

                if (ConfigData.TryGetValue("smrpg_exp_kill_max", out string? g_smrpg_exp_kill_max))
                {
                    g_hCVExpKillMax = uint.Parse(g_smrpg_exp_kill_max);
                }

                if (ConfigData.TryGetValue("smrpg_exp_teamwin", out string? g_smrpg_exp_teamwin))
                {
                    g_hCVExpTeamwin = bool.Parse(g_smrpg_exp_teamwin);
                }

                // ConfigData.TryGetValue("smrpg_exp_use_teamratio", "1", "Scale the experience for team events by the team ratio? This is e.g. used to lower the amount of experience earned, when a winning team has more players than the other.", 0, true, 0.0, true, 1.0);

                if (ConfigData.TryGetValue("smrpg_lastexperience_count", out string? g_smrpg_lastexperience_count))
                {
                    g_hCVLastExperienceCount = uint.Parse(g_smrpg_lastexperience_count);
                }

                if (ConfigData.TryGetValue("smrpg_level_start", out string? g_smrpg_level_start))
                {
                    g_hCVLevelStart = uint.Parse(g_smrpg_level_start);
                }

                if (ConfigData.TryGetValue("smrpg_level_start_give_credits", out string? g_smrpg_level_start_give_credits))
                {
                    g_hCVLevelStartGiveCredits = uint.Parse(g_smrpg_level_start_give_credits);
                }

                if (ConfigData.TryGetValue("smrpg_upgrade_start_levels_free", out string? g_smrpg_upgrade_start_levels_free))
                {
                    g_hCVUpgradeStartLevelsFree = bool.Parse(g_smrpg_upgrade_start_levels_free);
                }

                if (ConfigData.TryGetValue("smrpg_credits_inc", out string? g_smrpg_credits_inc))
                {
                    g_hCVCreditsInc = uint.Parse(g_smrpg_credits_inc);
                }

                if (ConfigData.TryGetValue("smrpg_credits_start", out string? g_smrpg_credits_start))
                {
                    g_hCVCreditsStart = uint.Parse(g_smrpg_credits_start);
                }

                if (ConfigData.TryGetValue("smrpg_sale_percent", out string? g_smrpg_sale_percent))
                {
                    g_hCVSalePercent = float.Parse(g_smrpg_sale_percent);
                }

                if (ConfigData.TryGetValue("smrpg_allow_sell_disabled_upgrade", out string? g_smrpg_allow_sell_disabled_upgrade))
                {
                    g_hCVAllowSellDisabled = bool.Parse(g_smrpg_allow_sell_disabled_upgrade);
                }

                if (ConfigData.TryGetValue("smrpg_ignore_level_barrier", out string? g_smrpg_ignore_level_barrier))
                {
                    g_hCVIgnoreLevelBarrier = bool.Parse(g_smrpg_ignore_level_barrier);
                }

                if (ConfigData.TryGetValue("smrpg_allow_present_upgrade_usage", out string? g_smrpg_allow_present_upgrade_usage))
                {
                    g_hCVAllowPresentUpgradeUsage = bool.Parse(g_smrpg_allow_present_upgrade_usage);
                }

                if (ConfigData.TryGetValue("smrpg_disable_level_selection", out string? g_smrpg_disable_level_selection))
                {
                    g_hCVDisableLevelSelection = bool.Parse(g_smrpg_disable_level_selection);
                }

                if (ConfigData.TryGetValue("smrpg_show_maxlevel_in_menu", out string? g_smrpg_show_maxlevel_in_menu))
                {
                    g_hCVShowMaxLevelInMenu = bool.Parse(g_smrpg_show_maxlevel_in_menu);
                }

                if (ConfigData.TryGetValue("smrpg_show_upgrades_teamlock", out string? g_smrpg_show_upgrades_teamlock))
                {
                    g_hCVShowUpgradesOfOtherTeam = bool.Parse(g_smrpg_show_upgrades_teamlock);
                }

                if (ConfigData.TryGetValue("smrpg_buy_upgrades_teamlock", out string? g_smrpg_buy_upgrades_teamlock))
                {
                    g_hCVBuyUpgradesOfOtherTeam = bool.Parse(g_smrpg_buy_upgrades_teamlock);
                }

                if (ConfigData.TryGetValue("smrpg_show_teamlock_notice_own_team", out string? g_smrpg_show_teamlock_notice_own_team))
                {
                    g_hCVShowTeamlockNoticeOwnTeam = bool.Parse(g_smrpg_show_teamlock_notice_own_team);
                }

                if (ConfigData.TryGetValue("smrpg_show_upgrade_purchase_in_chat", out string? g_smrpg_show_upgrade_purchase_in_chat))
                {
                    g_hCVShowUpgradePurchase = bool.Parse(g_smrpg_show_upgrade_purchase_in_chat);
                }

                if (ConfigData.TryGetValue("smrpg_show_menu_on_levelup", out string? g_smrpg_show_menu_on_levelup))
                {
                    g_hCVShowMenuOnLevelDefault = bool.Parse(g_smrpg_show_menu_on_levelup);
                }

                if (ConfigData.TryGetValue("smrpg_fade_screen_on_levelup", out string? g_smrpg_fade_screen_on_levelup))
                {
                    g_hCVFadeOnLevelDefault = bool.Parse(g_smrpg_fade_screen_on_levelup);
                }

                if (ConfigData.TryGetValue("smrpg_fade_screen_on_levelup_color", out string? g_smrpg_fade_screen_on_levelup_color))
                {
                    string[] colorParts = g_smrpg_fade_screen_on_levelup_color.Split(' ');
                    for (int i = 0; i < colorParts.Length; i++)
                    {
                        if (uint.TryParse(colorParts[i], out uint value))
                        {
                            g_hCVFadeOnLevelColor[i] = value;
                        }
                        else
                        {
                            Console.WriteLine($"Invalid number: {colorParts[i]}");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                return;
            }

            // TODO: Загрузка файла авто-конфигурации для фиксации игровых параметров (стамина, бхоп, аксилирейт и т.д.).

            // TODO: Меню и регистрация.

            // TODO: Регистрация форвардов. 

            // TODO: Инициализация настроек, улучшений, базы.

            // TODO: Инициализация файлов перевода.

            // TODO: Хук ивентов. player_spawn,player_death,round_end,player_disconnect.
        }
    }
}