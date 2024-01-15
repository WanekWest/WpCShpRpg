using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace WpCShpRpg
{
    public class WpCShpRpg : BasePlugin
    {
        // БИО.
        public override string ModuleName => "WpCShpRPG";
        public override string ModuleVersion => "0.0.1";

        private static Database database = null;
        private static Config config = null;
        private static PlayerData playerData = null;
        private static Upgrades upgrades = null;
        private static Menu menu = null;

        public override void Load(bool hotReload)
        {
            config = new Config();

            if (config.LoadModCondiguration(ModuleDirectory) == false)
            {
                Console.WriteLine("[CSSRPG] Ядро не было инициализировано!");
                return;
            }

            LoadExecutionFile();

            PlayerData playerData = new PlayerData();
            Upgrades upgrades = new Upgrades();
            Menu menu = new Menu();

            // TODO: Меню и регистрация.

            // TODO: Регистрация форвардов. 

            // TODO: Инициализация настроек, улучшений, базы.
            database = new Database(ModuleDirectory);
            database.InitDatabase();
            database.DatabaseMaid(config.g_hCVSaveData, config.g_hCVPlayerExpire);

            database.SetConfig(config);
            playerData.SetConfig(config);
            upgrades.SetConfig(config);
            menu.SetConfig(config);

            menu.SetDatabase(database);
            upgrades.SetDatabase(database);
            playerData.SetDatabase(database);

            menu.SetPlayerData(playerData);
            upgrades.SetPlayerData(playerData);
            database.SetPlayerData(playerData);

            database.SetMenu(menu);
            upgrades.SetMenu(menu);
            playerData.SetMenu(menu);

            database.SetUpgrades(upgrades);
            playerData.SetUpgrades(upgrades);
            menu.SetUpgrades(upgrades);

            // TODO: Инициализация файлов перевода.

            RegisterEventHandler<EventPlayerSpawn>(Event_OnPlayerSpawn);
            RegisterEventHandler<EventPlayerDeath>(Event_OnPlayerDeath);
            RegisterEventHandler<EventRoundEnd>(Event_OnRoundEnd);
            RegisterEventHandler<EventPlayerDisconnect>(Event_OnPlayerDisconnect);
        }

        #region Файл исполнения
        private void LoadExecutionFile()
        {
            // TODO: Переделать под считал строчку - выполнил, а не разбивать в словарик.
            string? ParentDirectory = Directory.GetParent(ModuleDirectory)?.Parent?.FullName;
            if (string.IsNullOrEmpty(ParentDirectory))
            {
                Console.WriteLine("Error: Unable to find the parent directory for the module.");
                return;
            }

            string configPath = Path.Combine(ParentDirectory, "configs/wpcshprpg_execute.cfg");
            if (!File.Exists(configPath))
            {
                Console.WriteLine("Error: Unable to find the parent directory for the module.");
                return;
            }

            Dictionary<string, string> ConfigData = Config.ParseConfigFile(configPath);
            foreach (var kvp in ConfigData)
            {
                Server.PrintToConsole($"{kvp.Key} {kvp.Value}");
                Console.WriteLine($"Применено: {kvp.Key}: {kvp.Value}");
            }

            return;
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
    }
}