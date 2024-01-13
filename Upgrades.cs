using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using System.Collections;
using CounterStrikeSharp.API.Core.Plugin;
using static WpCShpRpg.PlayerData;

namespace WpCShpRpg
{
    public class Upgrades
    {
        private Database database = null;
        private Config config = null;
        private PlayerData playerData = null;
        private Menu menu = null;

        public Upgrades(Database database, Config config, PlayerData playerData, Menu menu)
        {
            InitUpgrades();
            this.database = database;
            this.config = config;
            this.playerData = playerData;
            this.menu = menu;
        }

        public struct InternalUpgradeInfo
        {
            public uint index { get; set; }                      // Индекс в массиве g_hUpgrades
            public uint databaseId { get; set; }                  // upgrade_id в таблице upgrades
            public bool databaseLoading { get; set; }            // Загружается ли databaseId этого апгрейда?
            public bool enabled { get; set; }                    // Апгрейд включен?
            public bool unavailable { get; set; }                // Плагин, предоставляющий этот апгрейд, доступен?
            public uint maxLevelBarrier { get; set; }             // Верхний предел настройки maxlevel. Нельзя установить maxlevel выше этого.
            public uint maxLevel { get; set; }                    // Максимальный уровень, который может достигнуть игрок для этого апгрейда
            public uint startLevel { get; set; }                  // Уровень, с которого начинают игроки, когда впервые присоединяются к серверу.
            public uint startCost { get; set; }                   // Стоимость первого уровня в кредитах
            public int incCost { get; set; }                     // Стоимость каждого последующего уровня в кредитах
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

            public char[] name = new char[128];
            public char[] shortName = new char[128];
            public char[] description = new char[128];
            public InternalUpgradeInfo(uint index, uint databaseId, bool databaseLoading, bool enabled, bool unavailable,
                uint maxLevelBarrier, uint maxLevel, uint startLevel, uint startCost,
                int incCost, int adminFlag, bool enableVisuals, bool enableSounds, bool allowBots,
                int teamlock, Function queryCallback, Function activeCallback, Function translationCallback,
                Function resetCallback, Handle plugin, char[] name, char[] shortName, char[] description,
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

        public void InitUpgrades()
        {
            g_hUpgrades = new ArrayList();
        }

        public InternalUpgradeInfo GetUpgradeByIndex(int iIndex)
        {
            return (InternalUpgradeInfo)g_hUpgrades[iIndex];
        }

        public int GetUpgradeCount()
        {
            return g_hUpgrades.Count;
        }

        bool IsValidUpgrade(InternalUpgradeInfo upgrade)
        {
            // This plugin is available (again)?
            bool bUnavailable == PluginState.Loaded;
            if (upgrade.unavailable != bUnavailable)
            {
                upgrade.unavailable = bUnavailable;
                SaveUpgradeConfig(upgrade);
            }
            return !upgrade.unavailable;
        }

        void SaveUpgradeConfig(InternalUpgradeInfo upgrade)
        {
            g_hUpgrades.SetArray(upgrade.index, upgrade, sizeof(InternalUpgradeInfo));
        }

        int GetClientPurchasedUpgradeLevel(int client, int iUpgradeIndex)
        {
            PlayerUpgradeInfo playerupgrade = playerData.GetPlayerUpgradeInfoByIndex(client, iUpgradeIndex);
            return playerupgrade.purchasedlevel;
        }

        void SetClientPurchasedUpgradeLevel(int client, int iUpgradeIndex, int iLevel)
        {
            PlayerUpgradeInfo playerupgrade;
            playerData.GetPlayerUpgradeInfoByIndex(client, iUpgradeIndex, playerupgrade);
            // Differ for selected and purchased level!
            playerupgrade.purchasedlevel = iLevel;
            playerData.SavePlayerUpgradeInfo(client, iUpgradeIndex, playerupgrade);

            // Only update the selected level, if it's higher than the new limit
            int iSelectedLevel = GetClientSelectedUpgradeLevel(client, iUpgradeIndex);
            if (iSelectedLevel > iLevel)
                SetClientSelectedUpgradeLevel(client, iUpgradeIndex, iLevel);
        }

        void SetClientSelectedUpgradeLevel(int client, int iUpgradeIndex, int iLevel)
        {
            int iPurchased = GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);
            // Can't select a level he doesn't own yet.
            if (iPurchased < iLevel)
                return;

            int iOldLevel = GetClientSelectedUpgradeLevel(client, iUpgradeIndex);

            if (iLevel == iOldLevel)
                return;

            PlayerUpgradeInfo playerupgrade;
            GetPlayerUpgradeInfoByIndex(client, iUpgradeIndex, playerupgrade);
            // Differ for selected and purchased level!
            playerupgrade.selectedlevel = iLevel;
            SavePlayerUpgradeInfo(client, iUpgradeIndex, playerupgrade);

            // Don't call the callback, if the player disabled the upgrade.
            if (!playerupgrade.enabled)
                return;

            InternalUpgradeInfo upgrade;
            GetUpgradeByIndex(iUpgradeIndex, upgrade);

            if (!IsValidUpgrade(upgrade))
                return;

            // Plugin doesn't care? OK :(
            if (upgrade.queryCallback == INVALID_FUNCTION)
                return;

            if (IsClientInGame(client))
            {
                // Notify plugin about it.
                Call_StartFunction(upgrade.plugin, upgrade.queryCallback);
                Call_PushCell(client);
                Call_PushCell(iOldLevel < iLevel ? UpgradeQueryType_Buy : UpgradeQueryType_Sell);
                Call_Finish();
            }
        }

    }
}