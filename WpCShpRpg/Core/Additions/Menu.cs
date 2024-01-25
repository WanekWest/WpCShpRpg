using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using WpCShpRpg.Core.Additions;
using static WpCShpRpg.Core.Additions.PlayerData;
using static WpCShpRpgCoreApi.IWpCShpRpgCoreApi;

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

        private Database database;
        private ConfiguraionFiles config;
        private PlayerData playerData;
        private Upgrades upgrades;

        public bool IsRpgMenuCreated { get; private set; } = false;

        public bool IsBuyUpgradesMenuCreated { get; private set; } = false;

        public bool IsSellUpgradesMenuCreated { get; private set; } = false;

        public bool IsSettingsMenuCreated { get; private set; } = false;

        public bool IsHelpMenuCreated { get; private set; } = false;

        public Menu()
        {

        }

        public void SetDatabase(ref Database db)
        {
            database = db;
        }

        public void SetUpgrades(ref Upgrades upgrClass)
        {
            upgrades = upgrClass;
        }

        public void SetConfig(ref ConfiguraionFiles cfg)
        {
            config = cfg;
        }

        public void SetPlayerData(ref PlayerData pData)
        {
            playerData = pData;
        }

        public void CreateRpgMenu()
        {
            RpgMenu.AddMenuOption("Купить Навыки", (player, option) =>
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
                ShowLastExperianceMenuForPlayer(player);
            });
            RpgMenu.AddMenuOption("Меню администратора", (player, option) =>
            {
                // TODO: Доделать пункт, а над добавлением добавить проверку на флаг админский.
                // HACK: Вынести все проверки непосредственно в КсШарп.
                player.PrintToCenter("Это меню не для Вас!");
            });

            IsRpgMenuCreated = true;
        }

        public void ShowLastExperianceMenuForPlayer(CCSPlayerController? player)
        {
            CreateLastExperianceMenuForPlayer(player);
        }

        private void CreateLastExperianceMenuForPlayer(CCSPlayerController? player)
        {
            int client;
            if (player != null && player.UserId != null && player.UserId > 0)
            {
                client = Convert.ToInt32(player.UserId);
            }
            else
            {
                return;
            }

            List<int> hLastExperience = PlayerData.g_iPlayerSessionStartStats[client].LastExperience;

            int iSize = hLastExperience.Count;
            if (iSize > 0)
                return;

            Server.PrintToConsole($"Boobs size {iSize}");

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

        private void CreatePlayerSkillsMenu(CCSPlayerController? player, int TypeOfMethod)
        {
            int client;
            Server.PrintToChatAll($"CreatePlayerSkillsMenu {player.UserId} {player.Index} {player.DraftIndex}");
            Server.PrintToChatAll($"CreatePlayerSkillsMenu {player.UserId} {player.Index} {player.DraftIndex}");
            Server.PrintToChatAll($"CreatePlayerSkillsMenu {player.UserId} {player.Index} {player.DraftIndex}");

            if (player != null && player.UserId != null)
            {
                client = Convert.ToInt32(player.UserId);
            }
            else
            {
                return;
            }

            Server.PrintToChatAll($"Проверки прошел игрок");
            ChatMenu UpgradesMenu = new ChatMenu($"{ChatColors.DarkBlue}--[{ChatColors.Green}WP Навыки{ChatColors.DarkBlue}]--");
            for (int i = 0; i < Upgrades.GetUpgradeCount(); i++)
            {
                InternalUpgradeInfo CurrentUpgrade = Upgrades.GetUpgradeByIndex(i);

                if (Upgrades.IsValidUpgrade(CurrentUpgrade) == false)
                    return;

                Server.PrintToChatAll($"Сканю апгрейд {i} путь 2");
                try
                {
                    uint ClientUpgradeLevel = PlayerData.GetClientSelectedUpgradeLevel(client, i);
                    if (TypeOfMethod == 2 && ClientUpgradeLevel == 0)
                        continue;

                    UpgradesMenu.AddMenuOption($"{CurrentUpgrade.shortName} [{ClientUpgradeLevel}/{CurrentUpgrade.maxLevel}]", (player, option) =>
                    {
                        if (TypeOfMethod == 1)
                        {
                            Server.PrintToChatAll($"Чекаю путь 1 передаю {i} {client}");
                            ShowAdditionalUpgradeMenu(player, CurrentUpgrade, client);
                        }
                        else if (TypeOfMethod == 2)
                        {
                            Server.PrintToChatAll($"Чекаю путь 2");
                            ShowSellUpgradeMenu(player, CurrentUpgrade, client);
                        }
                        else
                        {
                            Server.PrintToChatAll($"Чекаю путь 3");
                            ShowSettingsUpgradeMenu(player, CurrentUpgrade, client);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Server.PrintToConsole($"Вылезла ошибка при g_iPlayerInfo: {ex.Message}");
                    Server.PrintToConsole($"Вылезла ошибка при g_iPlayerInfo: {ex.Message}");
                    Server.PrintToConsole($"Вылезла ошибка при g_iPlayerInfo: {ex.Message}");
                    Server.PrintToConsole($"Вылезла ошибка при g_iPlayerInfo: {ex.Message}");
                }
            }

            Server.PrintToChatAll($"Начинаю показывать меню");
            ChatMenus.OpenMenu(player, UpgradesMenu);
        }

        private void ShowAdditionalUpgradeMenu(CCSPlayerController player, InternalUpgradeInfo CurrentUpgrade, int Client)
        {
            ChatMenu AdditionalUpgradesMenu = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP {CurrentUpgrade.shortName}{ChatColors.DarkBlue}]--");

            uint iItemLevel = Upgrades.GetClientPurchasedUpgradeLevel(Client, CurrentUpgrade.index);
            uint iCost = Upgrades.GetUpgradeCost(CurrentUpgrade.index, iItemLevel + 1);

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
                if (PlayerData.BuyClientUpgrade(Client, CurrentUpgrade.index))
                {
                    if (config.g_hCVShowUpgradePurchase)
                    {
                        player.PrintToCenter("Навык прокачки повышен!");
                    }
                }
                else
                {
                    player.PrintToCenter("У вас недостаточно кредитов!");
                }

                AdditionalUpgradesMenu.AddMenuOption($"Ваш уровень скилла: {iItemLevel}", (player, option) =>
                {
                    option.Disabled = true;
                });

                AdditionalUpgradesMenu.AddMenuOption($"Максимальный уровень прокачки: {CurrentUpgrade.maxLevel}", (player, option) =>
                {
                    option.Disabled = true;
                });

                AdditionalUpgradesMenu.AddMenuOption($"Цена улучшения: {iCost}", (player, option) =>
                {
                    option.Disabled = true;
                });
            }

            ChatMenus.OpenMenu(player, AdditionalUpgradesMenu);
            return;
        }

        private void ShowSellUpgradeMenu(CCSPlayerController player, InternalUpgradeInfo CurrentUpgrade, int Client)
        {
            // ChatMenus.OpenMenu(player, SellUpgradesMenu);
            ConfirmSellMenu.AddMenuOption($"Да", (player, option) =>
            {
                if (playerData.SellClientUpgrade(Client, CurrentUpgrade.index))
                {
                    player.PrintToCenter("Навык продан");
                }
                else
                {
                    player.PrintToCenter("Навык не был продан");
                }

                ChatMenus.OpenMenu(player, RpgMenu);
            });

            ConfirmSellMenu.AddMenuOption($"Нет", (player, option) =>
            {
                ChatMenus.OpenMenu(player, RpgMenu);
            });

            ChatMenus.OpenMenu(player, ConfirmSellMenu);

            return;
        }

        private void ShowSettingsUpgradeMenu(CCSPlayerController player, InternalUpgradeInfo CurrentUpgrade, int Client)
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

            SettingsUpgradeMenu.AddMenuOption("Настройка уровня", (player, option) =>
            {
                ShowAddiitionalLevelSkillSettings(player, CurrentUpgrade.index, CurrentUpgrade, Client);
            });

            SettingsUpgradeMenu.AddMenuOption($"Настройка эффектов", (player, option) =>
            {
                ShowAddiitionalEffectSkillSettings(player, CurrentUpgrade.index, CurrentUpgrade, Client);
            });

            ChatMenus.OpenMenu(player, SettingsUpgradeMenu);
            return;
        }

        private void ShowAddiitionalLevelSkillSettings(CCSPlayerController player, int UpgradeIndex, InternalUpgradeInfo CurrentUpgrade, int Client)
        {
            uint SelectedLevel = PlayerData.GetClientSelectedUpgradeLevel(Client, UpgradeIndex);
            uint PurchasedLevel = PlayerData.GetClientPurchasedUpgradeLevel(Client, UpgradeIndex);
            ChatMenu AddiitionalLevelSkillSettings = new ChatMenu($" {ChatColors.DarkBlue}--[{ChatColors.Green}WP Настройка уровня навыка {CurrentUpgrade.shortName}{ChatColors.DarkBlue}]--");
            AddiitionalLevelSkillSettings.AddMenuOption($"Ваш уровень навыка: {SelectedLevel}/{PurchasedLevel}", (player, option) =>
            {
                option.Disabled = true;
            });

            if (SelectedLevel < PurchasedLevel)
            {
                AddiitionalLevelSkillSettings.AddMenuOption($"Повысить уровень", (player, option) =>
                {
                    if (config.g_hCVDisableLevelSelection == false)
                        Upgrades.SetClientSelectedUpgradeLevel(Client, UpgradeIndex, SelectedLevel + 1);
                });
            }

            if (SelectedLevel > 0)
            {
                AddiitionalLevelSkillSettings.AddMenuOption($"Понизить уровень", (player, option) =>
                {
                    if (config.g_hCVDisableLevelSelection == false)
                        Upgrades.SetClientSelectedUpgradeLevel(Client, UpgradeIndex, SelectedLevel - 1);
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
                    PlayerData.SavePlayerUpgradeInfo(Client, UpgradeIndex, playerupgrade);
                });

                if (bHasSounds)
                {
                    Effects = playerupgrade.sounds ? "Включить" : "Выключить";
                }
                AddiitionalEffectSkillSettings.AddMenuOption(Effects, (player, option) =>
                {
                    playerupgrade.sounds = playerupgrade.sounds ? false : true;
                    PlayerData.SavePlayerUpgradeInfo(Client, UpgradeIndex, playerupgrade);
                });
            }

            ChatMenus.OpenMenu(player, AddiitionalEffectSkillSettings);
            return;
        }
    }
}