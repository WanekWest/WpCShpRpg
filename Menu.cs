using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;

namespace WpCShpRpg
{
    public class Menu
    {
        public ChatMenu RpgMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP RPG{ChatColors.DarkBlue}]--");
        public ChatMenu BuyUpgradesMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Покупка навыков{ChatColors.DarkBlue}]--");
        public ChatMenu SellUpgradesMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Продажа навыков{ChatColors.DarkBlue}]--");
        public ChatMenu SettingsMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Настройки{ChatColors.DarkBlue}]--");
        public ChatMenu HelpMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Помощь{ChatColors.DarkBlue}]--");

        private static Database database = null;
        private static Config config = null;
        private static PlayerData playerData = null;
        private static Upgrades upgrades = null;

        public bool IsRpgMenuCreated { get; } = false;

        public bool IsBuyUpgradesMenuCreated { get; } = false;

        public bool IsSellUpgradesMenuCreated { get; } = false;

        public bool IsSettingsMenuCreated { get; } = false;

        public bool IsHelpMenuCreated { get; } = false;

        public Menu()
        {

        }

        public void SetDatabase(Database db)
        {
            database = db;
        }

        public void SetUpgrades(Upgrades upgrClass)
        {

            upgrades = upgrClass;
        }

        public void SetConfig(Config cfg)
        {
            config = cfg;
        }

        public void SetPlayerData(PlayerData pData)
        {
            playerData = pData;
        }

        private void CreateRpgMenu(CCSPlayerController? player)
        {
            if (player == null || !player.IsValid || player.IsBot || player.UserId <= 0)
            {
                return;
            }

            RpgMenu.AddMenuOption($"Навыки: ", (player, option) =>
            {

            });

            RpgMenu.AddMenuOption($"Продажа навыков: ", (player, option) =>
            {

            });

            RpgMenu.AddMenuOption($"Настройки навыков: ", (player, option) =>
            {

            });

            RpgMenu.AddMenuOption($"Статистика: ", (player, option) =>
            {

            });

            RpgMenu.AddMenuOption($"Настройки: ", (player, option) =>
            {

            });

            RpgMenu.AddMenuOption($"Помощь: ", (player, option) =>
            {

            });

            RpgMenu.AddMenuOption($"Меню: ", (player, option) =>
            {

            });
        }

        private void CreateRankMenu(CCSPlayerController? player)
        {
            CreateRankMenu(player);
        }

        private void CreateRPGInfoMenu(CCSPlayerController? player)
        {

        }

        private void CreateRPGTop10Menu(CCSPlayerController? player)
        {

        }

        private void CreateRPGNextMenu(CCSPlayerController? player)
        {
        }

        private void CreateRPGSessionMenu(CCSPlayerController? player)
        {
        }

        private void CreateRPGHelpMenu(CCSPlayerController? player)
        {

        }

        private void CreateRPGLatestExperienceMenu(CCSPlayerController? player)
        {

        }
    }
}
