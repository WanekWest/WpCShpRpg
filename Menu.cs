using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using static WpCShpRpg.PlayerData;
using static WpCShpRpg.Upgrades;

namespace WpCShpRpg
{
    public class Menu
    {
        public ChatMenu RpgMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP RPG{ChatColors.DarkBlue}]--");
        private ChatMenu BuyUpgradesMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Покупка навыков{ChatColors.DarkBlue}]--");
        private ChatMenu SellUpgradesMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Продажа навыков{ChatColors.DarkBlue}]--");
        private ChatMenu SettingsMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Настройки{ChatColors.DarkBlue}]--");
        private ChatMenu HelpMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Помощь{ChatColors.DarkBlue}]--");
        private ChatMenu StatsMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Статистика{ChatColors.DarkBlue}]--");

        private ChatMenu ConfirmResetStatsMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Выберите один из вариантов{ChatColors.DarkBlue}]--");
        private ChatMenu ConfirmSellMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Выберите один из вариантов{ChatColors.DarkBlue}]--");

        private static Database database;
        private static ConfiguraionFiles config;
        private static PlayerData playerData;
        private static Upgrades upgrades;

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

        public void SetConfig(ConfiguraionFiles cfg)
        {
            config = cfg;
        }

        public void SetPlayerData(PlayerData pData)
        {
            playerData = pData;
        }

        public void CreateRpgMenu()
        {
            RpgMenu.AddMenuOption("Навыки", (player, option) =>
            {
                CreatePlayerSkillsMenu(player, 1);
            });
            RpgMenu.AddMenuOption("Продать навыки", (player, option) =>
            {
                CreatePlayerSkillsMenu(player, 2);
            });
            RpgMenu.AddMenuOption("Настройка навыков", (player, option) =>
            {
                CreatePlayerSkillsMenu(player, 3);
            });
            RpgMenu.AddMenuOption("Помощь", (player, option) =>
            {
                // TODO: Тут Сброс, Инфа о навыках, Инфа о моде, Рекомендации по прокачке.
                player.PrintToCenter("В разработке!");
            });
            RpgMenu.AddMenuOption("Последний опыт", (player, option) =>
            {
                CreateLaseExperianceMenuForPlayer(player);
            });
            RpgMenu.AddMenuOption("Меню администратора", (player, option) =>
            {
                // TODO: Доделать пункт, а над добавлением добавить проверку на флаг админский.
                // HACK: Вынести все проверки непосредственно в КсШарп.
                player.PrintToCenter("Это меню не для Вас!");
            });
        }

        private void CreateLaseExperianceMenuForPlayer(CCSPlayerController? player)
        {
            int client;
            if (player != null && player.IsValid && !player.IsBot && player.UserId != null && player.UserId > 0)
            {
                client = Convert.ToInt32(player.UserId);
            }
            else
            {
                return;
            }

            List<int> hLastExperience = playerData.g_iPlayerSessionStartStats[client].LastExperience;

            int iSize = hLastExperience.Count;
            if (iSize > 0)
                return;

            ChatMenu LastExperianceMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Последний опыт{ChatColors.DarkBlue}]--");

            bool WasAnyExpFound = false;
            for (int i = 0; i < iSize; i++)
            {
                if (hLastExperience[i].ToString().Length <= 0)
                    break;

                ConfirmResetStatsMenu.AddMenuOption($"{hLastExperience[i]}", (player, option) =>
                {
                    ChatMenus.OpenMenu(player, SettingsMenu);
                });

                WasAnyExpFound = true;
            }

            if (!WasAnyExpFound)
            {
                ConfirmResetStatsMenu.AddMenuOption($"Вы не зарабатывали опыт за сессию!", (player, option) =>
                {

                });
            }

            ChatMenus.OpenMenu(player, LastExperianceMenu);
        }


        /// <summary>
        /// Создание меню с перечислением всех навыков игрока.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="TypeOfMethod">1-Покупка\n2-Проадажа\n3-настройка</param>
        private void CreatePlayerSkillsMenu(CCSPlayerController? player, int TypeOfMethod)
        {
            int client;
            if (player != null && player.IsValid && !player.IsBot && player.UserId != null && player.UserId > 0)
            {
                client = Convert.ToInt32(player.UserId);
            }
            else
            {
                return;
            }

            ChatMenu UpgradesMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Навыки{ChatColors.DarkBlue}]--");
            for (int i = 0; i < Upgrades.GetUpgradeCount(); i++)
            {
                InternalUpgradeInfo CurrentUpgrade = Upgrades.GetUpgradeByIndex(i);
                if (Upgrades.IsValidUpgrade(CurrentUpgrade) == false)
                    return;

                UpgradesMenu.AddMenuOption($"{CurrentUpgrade.shortName} {playerData.GetClientSelectedUpgradeLevel(client, i)}/{CurrentUpgrade.maxLevel}", (player, option) =>
                {
                    if (TypeOfMethod == 1)
                    {
                        ShowAdditionalUpgradeMenu(player, i, CurrentUpgrade, client);
                    }
                    else if (TypeOfMethod == 2)
                    {
                        ShowSellUpgradeMenu(player, i, client);
                    }
                    else
                    {
                        ShowSettingsUpgradeMenu(player, i, CurrentUpgrade, client);
                    }
                });
            }

            ChatMenus.OpenMenu(player, UpgradesMenu);
        }

        private void ShowAdditionalUpgradeMenu(CCSPlayerController player, int UpgradeIndex, InternalUpgradeInfo CurrentUpgrade, int Client)
        {
            ChatMenu AdditionalUpgradesMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP {CurrentUpgrade.shortName}{ChatColors.DarkBlue}]--");

            uint iItemLevel = upgrades.GetClientPurchasedUpgradeLevel(Client, UpgradeIndex);
            uint iCost = upgrades.GetUpgradeCost(UpgradeIndex, iItemLevel + 1);
            if (iItemLevel >= CurrentUpgrade.maxLevel)
            {
                player.PrintToCenter("У вас максимальный уровень прокачки!");
                return;
            }
            else if (playerData.GetClientCredits(Client) < iCost)
            {
                player.PrintToCenter("У вас недостаточно кредитов!");
                return;
            }
            else
            {
                if (PlayerData.BuyClientUpgrade(Client, UpgradeIndex))
                {
                    player.PrintToCenter("Навык прокачки повышен!");
                    if (config.g_hCVShowUpgradePurchase)
                    {
                        player.PrintToCenter("У вас недостаточно кредитов!");
                    }
                }
            }

            ChatMenus.OpenMenu(player, AdditionalUpgradesMenu);
            return;
        }

        private void ShowSellUpgradeMenu(CCSPlayerController player, int UpgradeIndex, int Client)
        {
            ChatMenus.OpenMenu(player, SellUpgradesMenu);
            ConfirmSellMenu.AddMenuOption($"Да", (player, option) =>
            {
                if (playerData.SellClientUpgrade(Client, UpgradeIndex))
                {
                    player.PrintToCenter("Навык продан");
                }

                ChatMenus.OpenMenu(player, RpgMenu);
            });

            ConfirmSellMenu.AddMenuOption($"Нет", (player, option) =>
            {
                ChatMenus.OpenMenu(player, RpgMenu);
            });

            return;
        }

        private void ShowSettingsUpgradeMenu(CCSPlayerController player, int UpgradeIndex, InternalUpgradeInfo CurrentUpgrade, int Client)
        {
            ChatMenu SettingsUpgradeMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Настройка навыков{CurrentUpgrade.shortName}{ChatColors.DarkBlue}]--");

            if (CurrentUpgrade.enabled)
            {
                SettingsUpgradeMenu.AddMenuOption($"Включить навык", (player, option) =>
                {
                    CurrentUpgrade.enabled = true;
                });
            }
            else
            {
                SettingsUpgradeMenu.AddMenuOption($"Выключить навык", (player, option) =>
                {
                    CurrentUpgrade.enabled = false;
                });
            }

            SettingsUpgradeMenu.AddMenuOption("Настройка уровная", (player, option) =>
            {
                ShowAddiitionalLevelSkillSettings(player, UpgradeIndex, CurrentUpgrade, Client);
            });

            SettingsUpgradeMenu.AddMenuOption($"Настройка эффектов", (player, option) =>
            {
                ShowAddiitionalEffectSkillSettings(player, UpgradeIndex, CurrentUpgrade, Client);
            });

            ChatMenus.OpenMenu(player, SettingsUpgradeMenu);
            return;
        }

        private void ShowAddiitionalLevelSkillSettings(CCSPlayerController player, int UpgradeIndex, InternalUpgradeInfo CurrentUpgrade, int Client)
        {
            uint SelectedLevel = playerData.GetClientSelectedUpgradeLevel(Client, UpgradeIndex);
            uint PurchasedLevel = playerData.GetClientPurchasedUpgradeLevel(Client, UpgradeIndex);
            ChatMenu AddiitionalLevelSkillSettings = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Настройка уровня навыка{CurrentUpgrade.shortName}{ChatColors.DarkBlue}]--");
            AddiitionalLevelSkillSettings.AddMenuOption($"Ваш уровень навыка: {SelectedLevel}/{PurchasedLevel}", (player, option) =>
            {
                option.Disabled = true;
            });

            if (SelectedLevel < PurchasedLevel)
            {
                AddiitionalLevelSkillSettings.AddMenuOption($"Повысить уровень", (player, option) =>
                {
                    if (config.g_hCVDisableLevelSelection == false)
                        upgrades.SetClientSelectedUpgradeLevel(Client, UpgradeIndex, SelectedLevel + 1);
                });
            }

            if (SelectedLevel > 0)
            {
                AddiitionalLevelSkillSettings.AddMenuOption($"Понизить уровень", (player, option) =>
                {
                    if (config.g_hCVDisableLevelSelection == false)
                        upgrades.SetClientSelectedUpgradeLevel(Client, UpgradeIndex, SelectedLevel - 1);
                });
            }

            ChatMenus.OpenMenu(player, AddiitionalLevelSkillSettings);
            return;
        }

        private void ShowAddiitionalEffectSkillSettings(CCSPlayerController player, int UpgradeIndex, InternalUpgradeInfo CurrentUpgrade, int Client)
        {
            ChatMenu AddiitionalEffectSkillSettings = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Настройка эффектов{CurrentUpgrade.shortName}{ChatColors.DarkBlue}]--");
            bool bHasVisuals = CurrentUpgrade.visualsConvar != null && CurrentUpgrade.enableVisuals;
            bool bHasSounds = CurrentUpgrade.soundsConvar != null && CurrentUpgrade.enableSounds;

            PlayerUpgradeInfo playerupgrade = PlayerData.GetPlayerUpgradeInfoByIndex(Client, UpgradeIndex);

            if (bHasVisuals || bHasSounds)
            {
                string Effects = "Включить";
                if (bHasVisuals)
                {
                    Effects = playerupgrade.visuals ? "Включить" : "Выключить";
                }

                AddiitionalEffectSkillSettings.AddMenuOption(Effects, (player, option) =>
                {
                    playerupgrade.visuals = playerupgrade.visuals ? false : true;
                    playerData.SavePlayerUpgradeInfo(Client, UpgradeIndex, playerupgrade);
                });

                if (bHasSounds)
                {
                    Effects = playerupgrade.sounds ? "Включить" : "Выключить";
                }
                AddiitionalEffectSkillSettings.AddMenuOption(Effects, (player, option) =>
                {
                    playerupgrade.sounds = playerupgrade.sounds ? false : true;
                    playerData.SavePlayerUpgradeInfo(Client, UpgradeIndex, playerupgrade);
                });
            }

            ChatMenus.OpenMenu(player, AddiitionalEffectSkillSettings);
            return;
        }
    }
}