using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using System.Collections;
using System.Reflection.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static WpCShpRpg.PlayerData;

namespace WpCShpRpg
{
    public class Upgrades
    {
        private static Database database;
        private static Config config;
        private static PlayerData playerData;
        private static Menu menu;

        public Upgrades()
        {
            g_hUpgrades = new ArrayList();
        }

        public void SetDatabase(Database db)
        {
            database = db;
        }

        public void SetConfig(Config cfg)
        {
            config = cfg;
        }

        public void SetMenu(Menu mn)
        {
            menu = mn;
        }

        public void SetPlayerData(PlayerData pData)
        {
            playerData = pData;
        }

        public enum UpgradeQueryType
        {
            Buy,
            Sell
        }

        public delegate void SetClientSelectedUpgradeLevelCallback(int client, UpgradeQueryType queryType);
        public SetClientSelectedUpgradeLevelCallback SetClientSelectedUpgradeLevelCall { get; set; }

        public struct InternalUpgradeInfo
        {
            public int index { get; set; }                      // Индекс в массиве g_hUpgrades
            public uint databaseId { get; set; }                  // upgrade_id в таблице upgrades
            public bool databaseLoading { get; set; }            // Загружается ли databaseId этого апгрейда?
            public bool enabled { get; set; }                    // Апгрейд включен?
            public bool unavailable { get; set; }                // Плагин, предоставляющий этот апгрейд, доступен?
            public uint maxLevelBarrier { get; set; }             // Верхний предел настройки maxlevel. Нельзя установить maxlevel выше этого.
            public uint maxLevel { get; set; }                    // Максимальный уровень, который может достигнуть игрок для этого апгрейда
            public uint startLevel { get; set; }                  // Уровень, с которого начинают игроки, когда впервые присоединяются к серверу.
            public uint startCost { get; set; }                   // Стоимость первого уровня в кредитах
            public uint incCost { get; set; }                     // Стоимость каждого последующего уровня в кредитах
            public int adminFlag { get; set; }                   // Администраторские флаги, к которым ограничен этот апгрейд
            public bool enableVisuals { get; set; }              // Включить визуальные эффекты этого апгрейда по умолчанию?
            public bool enableSounds { get; set; }               // Включить аудио эффекты этого апгрейда по умолчанию?
            public bool allowBots { get; set; }                  // Разрешено ли ботам использовать этот апгрейд?
            public int teamlock { get; set; }                    // Могут ли использовать этот апгрейд только игроки определенной команды?
            public Function queryCallback { get; set; }          // Обратный вызов, когда игрок купил/продал апгрейд
            public Function activeCallback { get; set; }         // Обратный вызов, чтобы узнать, находится ли игрок в данный момент под воздействием этого апгрейда
            public Function translationCallback { get; set; }    // Обратный вызов, когда предстоит отобразить имя апгрейда
            public Function resetCallback { get; set; }          // Обратный вызов, когда следует убрать эффект апгрейда
            public Handle plugin { get; set; }                   // Плагин, зарегистрировавший апгрейд

            // Convar handles to track changes and upgrade the right value in the cache
            public ConVar enableConvar { get; set; }
            public ConVar maxLevelConvar { get; set; }
            public ConVar startLevelConvar { get; set; }
            public ConVar startCostConvar { get; set; }
            public ConVar incCostConvar { get; set; }
            public ConVar adminFlagConvar { get; set; }
            public ConVar visualsConvar { get; set; }
            public ConVar soundsConvar { get; set; }
            public ConVar botsConvar { get; set; }
            public ConVar teamlockConvar { get; set; }

            public string name;
            public string shortName;
            public string description;

            public InternalUpgradeInfo(int index, uint databaseId, bool databaseLoading, bool enabled, bool unavailable,
                uint maxLevelBarrier, uint maxLevel, uint startLevel, uint startCost,
                uint incCost, int adminFlag, bool enableVisuals, bool enableSounds, bool allowBots,
                int teamlock, Function queryCallback, Function activeCallback, Function translationCallback,
                Function resetCallback, Handle plugin, string name, string shortName, string description,
                ConVar enableConvar, ConVar maxLevelConvar, ConVar startLevelConvar, ConVar startCostConvar,
                ConVar incCostConvar, ConVar adminFlagConvar, ConVar visualsConvar, ConVar soundsConvar,
                ConVar botsConvar, ConVar teamlockConvar)
            {
                this.index = index;
                this.databaseId = databaseId;
                this.databaseLoading = databaseLoading;
                this.enabled = enabled;
                this.unavailable = unavailable;
                this.maxLevelBarrier = maxLevelBarrier;
                this.maxLevel = maxLevel;
                this.startLevel = startLevel;
                this.startCost = startCost;
                this.incCost = incCost;
                this.adminFlag = adminFlag;
                this.enableVisuals = enableVisuals;
                this.enableSounds = enableSounds;
                this.allowBots = allowBots;
                this.teamlock = teamlock;
                this.queryCallback = queryCallback;
                this.activeCallback = activeCallback;
                this.translationCallback = translationCallback;
                this.resetCallback = resetCallback;
                this.plugin = plugin;
                this.name = name;
                this.shortName = shortName;
                this.description = description;
                this.enableConvar = enableConvar;
                this.maxLevelConvar = maxLevelConvar;
                this.startLevelConvar = startLevelConvar;
                this.startCostConvar = startCostConvar;
                this.incCostConvar = incCostConvar;
                this.adminFlagConvar = adminFlagConvar;
                this.visualsConvar = visualsConvar;
                this.soundsConvar = soundsConvar;
                this.botsConvar = botsConvar;
                this.teamlockConvar = teamlockConvar;
            }
        }

        ArrayList g_hUpgrades;

        public InternalUpgradeInfo GetUpgradeByIndex(int iIndex)
        {
            return (InternalUpgradeInfo)g_hUpgrades[iIndex];
        }

        public int GetUpgradeCount()
        {
            return g_hUpgrades.Count;
        }

        public bool IsValidUpgrade(InternalUpgradeInfo upgrade)
        {
            upgrade.unavailable = false;

            SaveUpgradeConfig(upgrade);

            return true;
        }

        void SaveUpgradeConfig(InternalUpgradeInfo upgrade)
        {
            g_hUpgrades[upgrade.index] = upgrade;
        }

        public uint GetClientPurchasedUpgradeLevel(int client, int iUpgradeIndex)
        {
            PlayerUpgradeInfo playerupgrade = playerData.GetPlayerUpgradeInfoByIndex(client, iUpgradeIndex);
            return playerupgrade.purchasedlevel;
        }

        public void SetClientPurchasedUpgradeLevel(int client, int iUpgradeIndex, uint iLevel)
        {
            PlayerUpgradeInfo playerupgrade = playerData.GetPlayerUpgradeInfoByIndex(client, iUpgradeIndex);
            // Differ for selected and purchased level!
            playerupgrade.purchasedlevel = iLevel;
            playerData.SavePlayerUpgradeInfo(client, iUpgradeIndex, playerupgrade);

            // Only update the selected level, if it's higher than the new limit
            uint iSelectedLevel = playerData.GetClientSelectedUpgradeLevel(client, iUpgradeIndex);
            if (iSelectedLevel > iLevel)
                SetClientSelectedUpgradeLevel(client, iUpgradeIndex, iLevel);
        }

        public void SetClientSelectedUpgradeLevel(int client, int iUpgradeIndex, uint iLevel)
        {
            uint iPurchased = GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);
            // Can't select a level he doesn't own yet.
            if (iPurchased < iLevel)
                return;

            uint iOldLevel = playerData.GetClientSelectedUpgradeLevel(client, iUpgradeIndex);

            if (iLevel == iOldLevel)
                return;

            PlayerUpgradeInfo playerupgrade = playerData.GetPlayerUpgradeInfoByIndex(client, iUpgradeIndex);
            // Differ for selected and purchased level!
            playerupgrade.selectedlevel = iLevel;
            playerData.SavePlayerUpgradeInfo(client, iUpgradeIndex, playerupgrade);

            // Don't call the callback, if the player disabled the upgrade.
            if (!playerupgrade.enabled)
                return;

            InternalUpgradeInfo upgrade = GetUpgradeByIndex(iUpgradeIndex);

            if (!IsValidUpgrade(upgrade))
                return;

            CCSPlayerController? player = Utilities.GetPlayerFromIndex(client);
            if (player != null && player.IsValid && !player.IsBot)
            {
                UpgradeQueryType queryType = iOldLevel < iLevel ? UpgradeQueryType.Buy : UpgradeQueryType.Sell;
                SetClientSelectedUpgradeLevelCall?.Invoke(client, queryType);
            }
        }

        public uint GetUpgradeCost(int iItemIndex, uint iLevel)
        {
            InternalUpgradeInfo upgrade = GetUpgradeByIndex(iItemIndex);
            if (iLevel <= 1)
                return upgrade.startCost;
            else
                return upgrade.startCost + upgrade.incCost * (iLevel - 1);
        }

        public uint GetUpgradeSale(int iItemIndex, uint iLevel)
        {
            uint iCost = GetUpgradeCost(iItemIndex, iLevel);

            float fSalePercent = config.g_hCVSalePercent;
            if (fSalePercent == 1.0)
                return iCost;

            if (iLevel <= 1)
                return iCost;

            uint iSale = (uint)Math.Floor(iCost * (fSalePercent > 1.0 ? (fSalePercent / 100.0) : fSalePercent) + 0.5);
            uint iCreditsInc = config.g_hCVCreditsInc;
            if (iCreditsInc <= 1)
                return iSale;
            else
                iSale = (uint)((iSale + Math.Floor(iCreditsInc / 2.0)) / iCreditsInc * iCreditsInc);

            if (iSale > iCost)
                return iCost;

            return iSale;
        }

        // Checks whether a client is in the correct team, if the upgrade is locked to one.
        public bool IsClientInLockedTeam(int client, InternalUpgradeInfo upgrade)
        {
            // This upgrade isn't locked at all. No restriction.
            if (upgrade.teamlock <= 1)
                return true;

            int iTeam;
            CCSPlayerController? player = Utilities.GetPlayerFromIndex(client);
            if (player != null && player.IsValid && !player.IsBot)
            {
                iTeam = player.TeamNum;
            }
            else
            {
                return false;
            }

            // Always grant access to all upgrades, if the player is in spectator mode.
            if (iTeam <= 1)
                return true;

            // See if the player is in the allowed team.
            return iTeam == upgrade.teamlock;
        }
    }
}