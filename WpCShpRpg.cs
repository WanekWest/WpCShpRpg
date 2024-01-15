using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;

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


        [ConsoleCommand("rpgmenu", "Opens the rpg main menu")]
        public void OnCommandRpgMenu(CCSPlayerController? player, CommandInfo command)
        {
            ShowRpgMenu(player);
        }

        [ConsoleCommand("rpg", "Opens the rpg main menu")]
        public void OnCommandRpg(CCSPlayerController? player, CommandInfo command)
        {
            ShowRpgMenu(player);
        }

        private void ShowRpgMenu(CCSPlayerController? player)
        {
            ChatMenus.OpenMenu(player, menu.RpgMenu);
        }

        [ConsoleCommand("rpgrank", "Shows your rank or the rank of the target person. rpgrank [name|steamid|#userid]")]
        public void Cmd_RPGRank(CCSPlayerController? player, CommandInfo command)
        {
            CreateRankMenu(player);
        }

        private void CreateRankMenu(CCSPlayerController? player)
        {
            CreateRankMenu(player);
        }

        [ConsoleCommand("rpginfo", "Shows the purchased upgrades of the target person. rpginfo <name|steamid|#userid>")]
        public void Cmd_RPGInfo(CCSPlayerController? player, CommandInfo command)
        {
            CreateRPGInfoMenu(player);
        }

        private void CreateRPGInfoMenu(CCSPlayerController? player)
        {

        }

        [ConsoleCommand("rpgtop10", "Show the SM:RPG top 10")]
        public void Cmd_RPGTop10(CCSPlayerController? player, CommandInfo command)
        {

            CreateRPGTop10Menu(player);
        }

        private void CreateRPGTop10Menu(CCSPlayerController? player)
        {

        }

        [ConsoleCommand("rpgnext", "Show the next few ranked players before you")]
        public void Cmd_RPGNext(CCSPlayerController? player, CommandInfo command)
        {
            CreateRPGNextMenu(player);
        }

        private void CreateRPGNextMenu(CCSPlayerController? player)
        {
        }

        [ConsoleCommand("rpgsession", "Show your session stats")]
        public void Cmd_RPGSession(CCSPlayerController? player, CommandInfo command)
        {
            CreateRPGSessionMenu(player);
        }

        private void CreateRPGSessionMenu(CCSPlayerController? player)
        {
        }

        [ConsoleCommand("rpghelp", "Show the SM:RPG help menu")]
        public void Cmd_RPGHelp(CCSPlayerController? player, CommandInfo command)
        {
            CreateRPGHelpMenu(player);
        }

        private void CreateRPGHelpMenu(CCSPlayerController? player)
        {

        }

        [ConsoleCommand("rpgexp", "Show the latest experience you earned")]
        public void Cmd_RPGLatestExperience(CCSPlayerController? player, CommandInfo command)
        {
            CreateRPGLatestExperienceMenu(player);
        }

        private void CreateRPGLatestExperienceMenu(CCSPlayerController? player)
        {

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
                // Server.PrintToConsole($"{kvp.Key} {kvp.Value}");
                Server.ExecuteCommand($"{kvp.Key} {kvp.Value}");
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