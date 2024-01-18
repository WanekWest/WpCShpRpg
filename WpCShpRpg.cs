using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;

namespace WpCShpRpg
{
    public class WpCShpRpg : BasePlugin
    {
        public override string ModuleName => "WpCShpRPG | Rpg Mode";
        public override string ModuleVersion => "0.0.1";
        public override string ModuleAuthor => "WanekWest";
        public override string ModuleDescription => "Инновационный РПГ мод для CS:2!";

        private static Database database;
        private static Config config;
        private static PlayerData playerData;
        private static Upgrades upgrades;
        private static Menu menu;

        public override void Load(bool hotReload)
        {
            config = new Config();

            if (config.LoadModCondiguration(ModuleDirectory) == false)
            {
                Server.PrintToConsole(("[CSSRPG] Ядро не было инициализировано!"));
                return;
            }

            LoadExecutionFile();

            PlayerData playerData = new PlayerData(ModuleDirectory);
            Upgrades upgrades = new Upgrades(ModuleDirectory);
            Menu menu = new Menu();
            menu.CreateRpgMenu();

            // TODO: Меню и регистрация.

            // TODO: Регистрация форвардов. 

            Server.PrintToConsole("пипипипипи");
            // TODO: Инициализация настроек, улучшений, базы.
            database = new Database(ModuleDirectory, config);
            database.InitDatabase();
            database.DatabaseMaid(config.g_hCVSaveData, config.g_hCVPlayerExpire);

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

            Server.PrintToConsole("реги");
            RegisterEventHandler<EventPlayerSpawn>(Event_OnPlayerSpawn);
            RegisterEventHandler<EventPlayerDeath>(Event_OnPlayerDeath);
            RegisterEventHandler<EventRoundEnd>(Event_OnRoundEnd);
            RegisterEventHandler<EventPlayerDisconnect>(Event_OnPlayerDisconnect);
            Server.PrintToConsole("стоп");
        }


        [ConsoleCommand("rpgmenu", "Opens the rpg main menu")]
        public void OnCommandRpgMenu(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                if(menu.IsHelpMenuCreated)
                ChatMenus.OpenMenu(player, menu.RpgMenu);
            }
        }

        [ConsoleCommand("rpg", "Opens the rpg main menu")]
        public void OnCommandRpg(CCSPlayerController? player, CommandInfo command)
        {
            if (player != null && player.IsValid && !player.IsBot)
            {
                if (menu.IsHelpMenuCreated)
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

        #region Файл исполнения
        private void LoadExecutionFile()
        {
            // TODO: Переделать под считал строчку - выполнил, а не разбивать в словарик.
            string? ParentDirectory = Directory.GetParent(ModuleDirectory)?.Parent?.FullName;
            if (string.IsNullOrEmpty(ParentDirectory))
            {
                Server.PrintToConsole("Error: Unable to find the parent directory for the module.");
                return;
            }

            string configPath = Path.Combine(ParentDirectory, "configs/wpcshprpg_execute.cfg");
            if (!File.Exists(configPath))
            {
                Server.PrintToConsole("Error: Unable to find the parent directory for the module.");
                return;
            }

            Dictionary<string, string> ConfigData = Config.ParseConfigFile(configPath);
            foreach (var kvp in ConfigData)
            {
                // Server.PrintToConsole($"{kvp.Key} {kvp.Value}");
                Server.ExecuteCommand($"{kvp.Key} {kvp.Value}");
                Server.PrintToConsole($"Применено: {kvp.Key}: {kvp.Value}");
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

        public Upgrades GetUpgradesClass()
        {
            return upgrades;
        }
    }
}