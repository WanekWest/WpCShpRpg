using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using Modularity;
using WpCShpRpg.Core.Additions;
using WpCShpRpgCoreApi;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpCShpRpg.Core
{
    public class WpCShpRpg : BasePlugin, ICorePlugin
    {
        public override string ModuleName => "WpCShpRPG | Rpg Mode";
        public override string ModuleVersion => "0.0.1";
        public override string ModuleAuthor => "WanekWest";
        public override string ModuleDescription => "Инновационный РПГ мод для CS:2!";

        private static Database database;
        private static ConfiguraionFiles config;
        private static PlayerData playerData;
        private static Upgrades upgrades;
        private static Menu menu;

        public static event Action OnRpgCoreLoaded;

        public static bool RpgCoreLoaded;

        public static string ModuleDirectoryImproved { get; private set; } = "";

        public override void Load(bool hotReload)
        {
            RpgCoreLoaded = false;

            const string targetSubPath = "/addons/counterstrikesharp/plugins/ModularityPlugin";
            int indexOfSubPath = ModuleDirectory.IndexOf(targetSubPath);
            if (indexOfSubPath != -1)
            {
                // Добавляем длину targetSubPath, чтобы включить сам targetSubPath в результат
                ModuleDirectoryImproved = ModuleDirectory.Substring(0, indexOfSubPath + targetSubPath.Length);
            }

            if (ModuleDirectoryImproved == "")
                ModuleDirectoryImproved = ModuleDirectory;

            config = new ConfiguraionFiles();

            if (config.LoadModCondiguration(ModuleDirectoryImproved) == false)
            {
                Server.PrintToConsole("[CSSRPG] Ядро не было инициализировано!");
                return;
            }

            config.LoadExecutionFile(ModuleDirectoryImproved);

            // TODO: Меню и регистрация.
            menu = new Menu();
            menu.CreateRpgMenu();

            // TODO: Регистрация форвардов. 


            // TODO: Инициализация настроек, улучшений, базы.
            try
            {
                database = new Database(config, config.LoadDatabaseConfig(ModuleDirectoryImproved));
                database.InitDatabase();
                database.DatabaseMaid(config.g_hCVSaveData, config.g_hCVPlayerExpire);
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Ошибка при инициализации базы данных: {ex.Message}");
                WorkWithDatabase();
            }

            menu.SetConfig(config);
            menu.SetDatabase(database);
            database.SetMenu(menu);

            // TODO: Инициализация файлов перевода.

            RegisterEventHandler<EventPlayerSpawn>(Event_OnPlayerSpawn);
            RegisterEventHandler<EventPlayerDeath>(Event_OnPlayerDeath);
            RegisterEventHandler<EventRoundEnd>(Event_OnRoundEnd);
            RegisterEventHandler<EventPlayerDisconnect>(Event_OnPlayerDisconnect);

            RegisterListener<Listeners.OnMapStart>(name =>
            {
                Server.PrintToConsole("OnMapStart OnMapStart 1");
                playerData = new PlayerData();
                playerData.SetConfig(config);
                playerData.SetDatabase(database);
                menu.SetPlayerData(playerData);
                database.SetPlayerData(playerData);
                playerData.SetMenu(menu);

                WorkWithUpgradesClass();

                RpgCoreLoaded = true;
                OnRpgCoreLoaded?.Invoke();

                LoadCore(new PluginApis());
            });

            RegisterListener<Listeners.OnClientConnected>(slot =>
            {
                playerData.InitPlayerSessionStartStats(slot);
            });
        }

        private void WorkWithUpgradesClass()
        {
            if (ModuleDirectoryImproved == null || ModuleDirectoryImproved == "")
            {
                AddTimer(2.0f, WorkWithUpgradesClass);
            }
            else
            {
                upgrades = new Upgrades(ModuleDirectoryImproved);
                upgrades.SetConfig(config);
                upgrades.SetDatabase(database);
                upgrades.SetPlayerData(playerData);
                upgrades.SetMenu(menu);

                database.SetUpgrades(upgrades);
                playerData.SetUpgrades(upgrades);
                menu.SetUpgrades(upgrades);
            }

            return;
        }

        private void WorkWithDatabase()
        {
            if (ModuleDirectory == null)
            {
                AddTimer(2.0f, WorkWithDatabase);
            }
            else
            {
                database = new Database(config, config.LoadDatabaseConfig(ModuleDirectoryImproved));
                database.InitDatabase();
                database.DatabaseMaid(config.g_hCVSaveData, config.g_hCVPlayerExpire);
            }

            return;
        }

        public void LoadCore(IApiRegisterer apiRegisterer)
        {
            Console.WriteLine("Loading WpCssRpg Core");
            apiRegisterer.Register<IWpCShpRpgCoreApi>(new WpCShpRpgCoreApi());
        }

        [GameEventHandler]
        public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;

            if (player == null || player.UserId < 0 || !player.IsValid || player.UserId == null)
                return HookResult.Continue;

            if (config.g_hCVEnable)
                PlayerData.InitPlayer((int)player.UserId);

            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnClientAuthorized(EventPlayerConnectFull @event, GameEventInfo info)
        {
            if (@event.Userid != null && !@event.Userid.IsValid)
                return HookResult.Continue;

            if (!config.g_hCVEnable)
                return HookResult.Continue;

            CCSPlayerController? player = Utilities.GetPlayerFromIndex((int)@event.Userid.Index);
            if (player == null || player.UserId <= 0 || !player.IsValid || player.UserId == null)
                return HookResult.Continue;

            string query;
            if (player.IsBot)
            {
                if (!config.g_hCVBotSaveStats)
                    return HookResult.Continue;

                if (player.IsHLTV)
                    return HookResult.Continue;

                // Экранирование имени для безопасности запроса
                string escapedName = player.PlayerName.Replace("'", "''");
                Server.PrintToConsole($"escapedName is {escapedName}");
                query = $"SELECT player_id, level, experience, credits, lastreset, lastseen, showmenu, fadescreen FROM players WHERE steamid IS NULL AND name = {escapedName} ORDER BY level DESC LIMIT 1";
            }
            else
            {
                ulong accountId = player.SteamID;
                if (accountId == 0)
                    return HookResult.Continue;

                Server.PrintToConsole($"accountId is {accountId}");
                query = $"SELECT player_id, level, experience, credits, lastreset, lastseen, showmenu, fadescreen FROM players WHERE steamid = {accountId} ORDER BY level DESC LIMIT 1";
            }

            database.GetPlayerInfo(query, (int)@event.Userid.Index);
            return HookResult.Continue;
        }

        #region Меню
        [ConsoleCommand("rpgmenu", "Opens the rpg main menu")]
        public void OnCommandRpgMenu(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                if (Menu.IsRpgMenuCreated)
                    ChatMenus.OpenMenu(player, menu.RpgMenu);
            }
        }

        [ConsoleCommand("rpg", "Opens the rpg main menu")]
        public void OnCommandRpg(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                if (Menu.IsRpgMenuCreated)
                    ChatMenus.OpenMenu(player, menu.RpgMenu);
            }
        }

        [ConsoleCommand("rpgrank", "Shows your rank or the rank of the target person. rpgrank [name|steamid|#userid]")]
        public void Cmd_RPGRank(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CreateRankMenu(player);
            }
        }

        private void CreateRankMenu(CCSPlayerController? player)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CreateRankMenu(player);
            }
        }

        [ConsoleCommand("rpginfo", "Shows the purchased upgrades of the target person. rpginfo <name|steamid|#userid>")]
        public void Cmd_RPGInfo(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CreateRPGInfoMenu(player);
            }
        }

        private void CreateRPGInfoMenu(CCSPlayerController? player)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
            }
        }

        [ConsoleCommand("rpgtop10", "Show the SM:RPG top 10")]
        public void Cmd_RPGTop10(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CreateRPGTop10Menu(player);
            }
        }

        private void CreateRPGTop10Menu(CCSPlayerController? player)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
            }
        }

        [ConsoleCommand("rpgnext", "Show the next few ranked players before you")]
        public void Cmd_RPGNext(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CreateRPGNextMenu(player);
            }
        }

        private void CreateRPGNextMenu(CCSPlayerController? player)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
            }
        }

        [ConsoleCommand("rpgsession", "Show your session stats")]
        public void Cmd_RPGSession(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CreateRPGSessionMenu(player);
            }
        }

        private void CreateRPGSessionMenu(CCSPlayerController? player)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
            }
        }

        [ConsoleCommand("rpghelp", "Show the SM:RPG help menu")]
        public void Cmd_RPGHelp(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CreateRPGHelpMenu(player);
            }
        }

        private void CreateRPGHelpMenu(CCSPlayerController? player)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
            }
        }

        [ConsoleCommand("rpgexp", "Show the latest experience you earned")]
        public void Cmd_RPGLatestExperience(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                CreateRPGLatestExperienceMenu(player);
            }
        }

        private void CreateRPGLatestExperienceMenu(CCSPlayerController? player)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
            }

        }
        #endregion

        #region События
        private HookResult Event_OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            CCSPlayerController? Player = @event.Userid;
            if (Player == null || Player.UserId <= 0 || !Player.IsValid || Player.IsBot || Player.UserId == null)
                return HookResult.Continue;

            int? Client = Player.UserId;

            return HookResult.Continue;
        }

        public HookResult Event_OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            if (@event.Attacker == null || @event.Attacker.UserId == null || @event.Userid == null || @event.Userid.UserId == null)
                return HookResult.Continue;

            int? Attacker = @event.Attacker.UserId;
            int? Victim = @event.Userid.UserId;

            if (Attacker != null && Victim != null && Attacker <= 0 || Victim <= 0)
                return HookResult.Continue;

            return HookResult.Continue;
        }

        public HookResult Event_OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            return HookResult.Continue;
        }

        public HookResult Event_OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController? Player = @event.Userid;
            if (Player == null || Player.UserId <= 0 || !Player.IsValid || Player.IsBot || Player.UserId == null)
                return HookResult.Continue;

            int? Client = Player.UserId;

            return HookResult.Continue;
        }
        #endregion

        public Upgrades GetUpgradesClass()
        {
            return upgrades;
        }
    }

    public class WpCShpRpgCoreApi : IWpCShpRpgCoreApi
    {
        public bool IsRpgCoreLoaded()
        {
            return WpCShpRpg.RpgCoreLoaded;
        }

        public bool UpgradeExists(string ShortName)
        {
            return Upgrades.UpgradeExists(ShortName);
        }

        public void RegisterUpgradeType(string sName, string sShortName, string sDescription, uint iMaxLevelBarrier, bool bDefaultEnable,
             uint iDefaultMaxLevel, uint iDefaultStartCost, uint iDefaultCostInc, uint iDefaultAdminFlags, Function queryCallback, Function activeCallback)
        {
            Upgrades.RegisterUpgradeType(sName, sShortName, sDescription, iMaxLevelBarrier, bDefaultEnable,
             iDefaultMaxLevel, iDefaultStartCost, iDefaultCostInc, iDefaultAdminFlags, queryCallback, activeCallback);

            return;
        }

        public void UnregisterUpgradeType(string ShortName)
        {
            Upgrades.UnregisterUpgradeType(ShortName);

            return;
        }

        public Dictionary<string, string> GetParamsFromConfig(string ModuleDirectory, string ShortSkillName)
        {
            return GetParamsFromConfig(ModuleDirectory, ShortSkillName);
        }

        public string GetModuleDirectoryImproved()
        {
            return WpCShpRpg.ModuleDirectoryImproved;
        }
    }
}