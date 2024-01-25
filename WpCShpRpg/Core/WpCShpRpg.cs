using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using Modularity;
using WpCShpRpg.Core.Additions;
using WpCShpRpgCoreApi;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static WpCShpRpgCoreApi.IWpCShpRpgCoreApi;

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
        public static WpCShpRpgCoreApi CoreApi;

        public string ModuleDirectoryImproved { get; private set; } = "";

        public override void Load(bool hotReload)
        {
            try
            {
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

                menu = new Menu();
                menu.CreateRpgMenu();

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

                LoadCore(new PluginApis());

                menu.SetConfig(ref config);
                menu.SetDatabase(ref database);
                database.SetMenu(ref menu);

                Server.PrintToConsole("Ядро загружено 1!");
                Server.PrintToConsole("Ядро загружено 1!");
                Server.PrintToConsole("Ядро загружено 1!");
                Server.PrintToConsole("Ядро загружено 1!");
                Server.PrintToConsole("Ядро загружено 1!");

                // TODO: Инициализация файлов перевода.

                RegisterEventHandler<EventPlayerSpawn>(Event_OnPlayerSpawn);
                RegisterEventHandler<EventPlayerDeath>(Event_OnPlayerDeath);
                RegisterEventHandler<EventRoundEnd>(Event_OnRoundEnd);
                RegisterEventHandler<EventPlayerDisconnect>(Event_OnPlayerDisconnect);

                RegisterListener<Listeners.OnMapStart>(name =>
                {
                    playerData = new PlayerData();
                    playerData.SetConfig(ref config);
                    playerData.SetDatabase(ref database);
                    menu.SetPlayerData(ref playerData);
                    database.SetPlayerData(ref playerData);
                    playerData.SetMenu(ref menu);

                    WorkWithUpgradesClass();

                    CoreApi.CssRpg_CoreLoaded();
                });

                RegisterListener<Listeners.OnClientConnected>(slot =>
                {
                    playerData.InitPlayerSessionStartStats(slot);
                });
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Ошибка ядра: {ex.Message}");
                Server.PrintToConsole($"Ошибка ядра: {ex.Message}");
                Server.PrintToConsole($"Ошибка ядра: {ex.Message}");
            }
        }

        private void WorkWithUpgradesClass()
        {
            upgrades = new Upgrades(ModuleDirectoryImproved);
            upgrades.SetConfig(ref config);
            upgrades.SetDatabase(ref database);
            upgrades.SetPlayerData(ref playerData);
            upgrades.SetMenu(ref menu);
            // upgrades.SetWpCShpRpgCoreApi(CoreApi);

            database.SetUpgrades(ref upgrades);
            playerData.SetUpgrades(ref upgrades);
            menu.SetUpgrades(ref upgrades);

            return;
        }

        private void WorkWithDatabase()
        {
            database = new Database(config, config.LoadDatabaseConfig(ModuleDirectoryImproved));
            database.InitDatabase();
            database.DatabaseMaid(config.g_hCVSaveData, config.g_hCVPlayerExpire);

            return;
        }

        public void LoadCore(IApiRegisterer apiRegisterer)
        {
            try
            {
                Server.PrintToConsole("Loading WpCssRpg Core");
                CoreApi = new WpCShpRpgCoreApi(ref config, ModuleDirectoryImproved);
                apiRegisterer.Register<IWpCShpRpgCoreApi>(CoreApi);
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Ошибка LoadCore: {ex.Message}");
                Server.PrintToConsole($"Ошибка LoadCore: {ex.Message}");
                Server.PrintToConsole($"Ошибка LoadCore: {ex.Message}");
            }
        }

        [GameEventHandler]
        public HookResult EventPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
        {
            if (@event != null && @event.Userid != null && !@event.Userid.IsValid)
                return HookResult.Continue;

            CCSPlayerController? player = @event?.Userid;

            if (player == null || player.UserId < 0 || !player.IsValid || player.UserId == null && !player.IsBot && !player.IsHLTV)
                return HookResult.Continue;

            Server.PrintToConsole("InitPlayer InitPlayer");
            Server.PrintToConsole("InitPlayer InitPlayer");
            Server.PrintToConsole("InitPlayer InitPlayer");
            Server.PrintToConsole("InitPlayer InitPlayer");
            Server.PrintToConsole("InitPlayer InitPlayer");
            Server.PrintToConsole("InitPlayer InitPlayer");
            Server.PrintToConsole("InitPlayer InitPlayer");
            Server.PrintToConsole("InitPlayer InitPlayer");

            if (config.g_hCVEnable)
                playerData.InitPlayer((int)player.UserId);

            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult EventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
        {
            if (@event != null && @event.Userid != null && !@event.Userid.IsValid)
                return HookResult.Continue;

            if (!config.g_hCVEnable)
                return HookResult.Continue;

            CCSPlayerController? player = Utilities.GetPlayerFromIndex((int)@event.Userid.Index);
            if (player == null || player.UserId <= 0 || !player.IsValid || player.UserId == null && !player.IsBot && !player.IsHLTV)
                return HookResult.Continue;

            Server.PrintToConsole($"Method OnClientAuthorized and player.SteamID is {player.SteamID} and id {(int)@event.Userid.Index}");
            Server.PrintToConsole($"Method OnClientAuthorized and player.SteamID is {player.SteamID} and id {(int)@event.Userid.Index}");
            Server.PrintToConsole($"Method OnClientAuthorized and player.SteamID is {player.SteamID} and id {(int)@event.Userid.Index}");
            Server.PrintToConsole($"Method OnClientAuthorized and player.SteamID is {player.SteamID} and id {(int)@event.Userid.Index}");
            Server.PrintToConsole($"Method OnClientAuthorized and player.SteamID is {player.SteamID} and id {(int)@event.Userid.Index}");
            Server.PrintToConsole($"Method OnClientAuthorized and player.SteamID is {player.SteamID} and id {(int)@event.Userid.Index}");
            Server.PrintToConsole($"Method OnClientAuthorized and player.SteamID is {player.SteamID} and id {(int)@event.Userid.Index}");
            Server.PrintToConsole($"Method OnClientAuthorized and player.SteamID is {player.SteamID} and id {(int)@event.Userid.Index}");

            string query;
            if (player.IsBot)
            {
                if (!config.g_hCVBotSaveStats || player.IsHLTV)
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
                if (menu.IsRpgMenuCreated)
                    ChatMenus.OpenMenu(player, menu.RpgMenu);
            }
        }

        [ConsoleCommand("rpg", "Opens the rpg main menu")]
        public void OnCommandRpg(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                if (menu.IsRpgMenuCreated)
                    ChatMenus.OpenMenu(player, menu.RpgMenu);
            }
        }

        [ConsoleCommand("rpgrank", "Shows your rank")]
        public void Cmd_RPGRank(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot && player.UserId != null)
            {
                player.PrintToChat($"Ваш ранг {Database.GetPlayerRank(player)} из {Database.GetAmountOfRanks()}");
            }
        }


        [ConsoleCommand("rpginfo", "Shows the purchased upgrades of the target person. rpginfo <name|steamid|#userid>")]
        public void Cmd_RPGInfo(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                // CreateRPGInfoMenu(player);
            }
        }

        [ConsoleCommand("rpgtop10", "Show the SM:RPG top 10")]
        public void Cmd_RPGTop10(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                // CreateRPGTop10Menu(player);
            }
        }

        [ConsoleCommand("rpgnext", "Show the next few ranked players before you")]
        public void Cmd_RPGNext(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                // CreateRPGNextMenu(player);
            }
        }

        [ConsoleCommand("rpgsession", "Show your session stats")]
        public void Cmd_RPGSession(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                // CreateRPGSessionMenu(player);
            }
        }

        [ConsoleCommand("rpghelp", "Show the SM:RPG help menu")]
        public void Cmd_RPGHelp(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                // CreateRPGHelpMenu(player);
            }
        }

        [ConsoleCommand("rpgexp", "Show the latest experience you earned")]
        public void Cmd_RPGLatestExperience(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                menu.ShowLastExperianceMenuForPlayer(player);
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

        public static Upgrades GetUpgradesClass()
        {
            return upgrades;
        }

        public static ConfiguraionFiles GetConfig()
        {
            return config;
        }
    }

    public class WpCShpRpgCoreApi : IWpCShpRpgCoreApi
    {
        private ConfiguraionFiles config;
        private string ModuleDirectoryImproved;

        public WpCShpRpgCoreApi(ref ConfiguraionFiles cfg, string moduleDirectoryImproved)
        {
            config = cfg;
            ModuleDirectoryImproved = moduleDirectoryImproved;
        }

        public event Action CssRpg_OnCoreLoaded;

        public void CssRpg_CoreLoaded()
        {
            if (CssRpg_OnCoreLoaded != null)
                CssRpg_OnCoreLoaded?.Invoke();
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

        public string GetModuleDirectoryImproved()
        {
            return ModuleDirectoryImproved;
        }

        public bool CssRpg_IsEnabled()
        {
            return config.g_hCVEnable;
        }

        public event Action<int, IWpCShpRpgCoreApi.UpgradeQueryType, string>? CssRpg_UpgradeBuySell;

        public void CssRpg_BuySell(int client, IWpCShpRpgCoreApi.UpgradeQueryType queryType, string UpgradeShortName)
        {
            if (CssRpg_UpgradeBuySell != null)
            {
                Server.PrintToChatAll("CssRpg_UpgradeBuySell?.Invoke(client, queryType, UpgradeShortName);");
                CssRpg_UpgradeBuySell?.Invoke(client, queryType, UpgradeShortName);
                Server.PrintToChatAll("end");
            }
        }

        public uint GetClientUpgradeLevel(int client, string shortname)
        {
            return PlayerData.GetClientUpgradeLevel(client, shortname);
        }

        public bool GetUpgradeByShortname(string sShortName, ref InternalUpgradeInfo upgrade)
        {
            return Upgrades.GetUpgradeByShortname(sShortName, ref upgrade);
        }

        public event Action<int, string, uint>? OnBuyUpgrade;

        public void CssRpg_OnBuyUpgrade(int client, string shortName, uint currentLevel)
        {
            if (OnBuyUpgrade != null)
                OnBuyUpgrade?.Invoke(client, shortName, currentLevel);
        }

        public event Action<int, string, uint>? BuyUpgradePost;

        public void CssRpg_BuyUpgradePost(int client, string shortName, uint currentLevel)
        {
            if (BuyUpgradePost != null)
                BuyUpgradePost?.Invoke(client, shortName, currentLevel);
        }

        public event Action<int, string, uint>? SellUpgrade;

        public void CssRpg_SellUpgrade(int client, string shortName, uint iCurrentLevel)
        {
            if (SellUpgrade != null)
                SellUpgrade?.Invoke(client, shortName, iCurrentLevel);
        }

        public event Action<int, string, uint>? SellUpgradePost;

        public void CssRpg_SellUpgradePost(int client, string shortName, uint currentLevel)
        {
            if (SellUpgradePost != null)
                SellUpgradePost?.Invoke(client, shortName, currentLevel);
        }

        public event Action<int, uint, uint>? ActionClientCredits;

        public void CssRpg_ClientCredits(int client, uint ClientCredits, uint iCredits)
        {
            if (ActionClientCredits != null)
                ActionClientCredits?.Invoke(client, ClientCredits, iCredits);
        }

        public event Action<int, uint, uint>? ClientCreditsPost;

        public void CssRpg_ClientCreditsPost(int client, uint iOldCredits, uint iCredits)
        {
            if (ClientCreditsPost != null)
                ClientCreditsPost?.Invoke(client, iOldCredits, iCredits);
        }

        public event Action<int, uint, uint>? ActionClientLevel;

        public void CssRpg_ClientLevel(int client, uint ClientLevel, uint iLevel)
        {
            if (ActionClientLevel != null)
                ActionClientLevel?.Invoke(client, ClientLevel, iLevel);
        }

        public event Action<int, uint, uint>? ClientLevelPost;

        public void CssRpg_ClientLevelPost(int client, uint iOldLevel, uint currentLevel)
        {
            if (ClientLevelPost != null)
                ClientLevelPost?.Invoke(client, iOldLevel, currentLevel);
        }

        public event Action<int, uint, uint>? ActionClientExperience;

        public void CssRpg_ClientExperience(int client, uint ClientExperience, uint iExperience)
        {
            if (ActionClientExperience != null)
                ActionClientExperience?.Invoke(client, ClientExperience, iExperience);
        }

        public event Action<int, uint, uint>? ActionClientExperiencePost;

        public void CssRpg_ClientExperiencePost(int client, uint ClientExperiencePost, uint currentLevel)
        {
            if (ActionClientExperiencePost != null)
                ActionClientExperiencePost?.Invoke(client, ClientExperiencePost, currentLevel);
        }

        public event Action<string>? OnUpgradeRegistered;

        public void CssRpg_OnUpgradeRegistered(string shortName)
        {
            if (OnUpgradeRegistered != null)
                OnUpgradeRegistered?.Invoke(shortName);
        }
    }
}