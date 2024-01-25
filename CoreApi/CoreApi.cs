using CounterStrikeSharp.API.Modules.Cvars;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpCShpRpgCoreApi
{
    public interface IWpCShpRpgCoreApi
    {
        enum UpgradeQueryType
        {
            Buy = 0,
            Sell = 1
        }

        public struct InternalUpgradeInfo
        {
            public int index { get; set; }                      // Индекс в массиве g_hUpgrades
            public int databaseId { get; set; }                  // upgrade_id в таблице upgrades
            public bool databaseLoading { get; set; }            // Загружается ли databaseId этого апгрейда?
            public bool enabled { get; set; }                    // Апгрейд включен?
            public bool unavailable { get; set; }                // Плагин, предоставляющий этот апгрейд, доступен?
            public uint maxLevelBarrier { get; set; }             // Верхний предел настройки maxlevel. Нельзя установить maxlevel выше этого.
            public uint maxLevel { get; set; }                    // Максимальный уровень, который может достигнуть игрок для этого апгрейда
            public uint startLevel { get; set; }                  // Уровень, с которого начинают игроки, когда впервые присоединяются к серверу.
            public uint startCost { get; set; }                   // Стоимость первого уровня в кредитах
            public uint incCost { get; set; }                     // Стоимость каждого последующего уровня в кредитах
            public string adminFlag { get; set; }                   // Администраторские флаги, к которым ограничен этот апгрейд
            public bool enableVisuals { get; set; }              // Включить визуальные эффекты этого апгрейда по умолчанию?
            public bool enableSounds { get; set; }               // Включить аудио эффекты этого апгрейда по умолчанию?
            public bool allowBots { get; set; }                  // Разрешено ли ботам использовать этот апгрейд?
            public uint teamlock { get; set; }                    // Могут ли использовать этот апгрейд только игроки определенной команды?
            public Function queryCallback { get; set; }          // Обратный вызов, когда игрок купил/продал апгрейд
            public Function activeCallback { get; set; }         // Обратный вызов, чтобы узнать, находится ли игрок в данный момент под воздействием этого апгрейда
            public Function translationCallback { get; set; }    // Обратный вызов, когда предстоит отобразить имя апгрейда
            public Function resetCallback { get; set; }          // Обратный вызов, когда следует убрать эффект апгрейда
            // public Handle plugin { get; set; }                   // Плагин, зарегистрировавший апгрейд

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

            public Dictionary<string, string> parameters { get; set; }

            public string name;
            public string shortName;
            public string description;

            public InternalUpgradeInfo(int index, int databaseId, bool databaseLoading, bool enabled, bool unavailable,
                uint maxLevelBarrier, uint maxLevel, uint startLevel, uint startCost,
                uint incCost, string adminFlag, bool enableVisuals, bool enableSounds, bool allowBots,
                uint teamlock, Function queryCallback, Function activeCallback, Function translationCallback,
                Function resetCallback, string name, string shortName, string description,
                ConVar enableConvar, ConVar maxLevelConvar, ConVar startLevelConvar, ConVar startCostConvar,
                ConVar incCostConvar, ConVar adminFlagConvar, ConVar visualsConvar, ConVar soundsConvar,
                ConVar botsConvar, ConVar teamlockConvar, Dictionary<string, string> parameters)
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
                this.parameters = parameters;
            }
        }

        public event Action CssRpg_OnCoreLoaded;

        public void CssRpg_CoreLoaded();

        public bool UpgradeExists(string ShortName);

        public void RegisterUpgradeType(string Name, string ShortName, string Description, uint MaxLevelBarrier, bool DefaultEnable,
             uint DefaultMaxLevel, uint DefaultStartCost, uint DefaultCostInc, uint DefaultAdminFlags, Function queryCallback, Function activeCallback);

        public void UnregisterUpgradeType(string ShortName);

        public string GetModuleDirectoryImproved();

        public bool CssRpg_IsEnabled();

        public event Action<int, UpgradeQueryType, string>? CssRpg_UpgradeBuySell;

        public uint GetClientUpgradeLevel(int client, string shortname);

        public bool GetUpgradeByShortname(string sShortName, ref InternalUpgradeInfo upgrade);

        //
        //
        //
        //
        public event Action<int, string, uint>? OnBuyUpgrade;

        public void CssRpg_OnBuyUpgrade(int client, string shortName, uint currentLevel);

        public event Action<int, string, uint>? BuyUpgradePost;

        public void CssRpg_BuyUpgradePost(int client, string shortName, uint currentLevel);

        public event Action<int, string, uint>? SellUpgrade;

        public void CssRpg_SellUpgrade(int client, string shortName, uint iCurrentLevel);

        public event Action<int, string, uint>? SellUpgradePost;

        public void CssRpg_SellUpgradePost(int client, string shortName, uint currentLevel);

        public event Action<int, uint, uint>? ActionClientCredits;

        public void CssRpg_ClientCredits(int client, uint ClientCredits, uint iCredits);

        public event Action<int, uint, uint>? ClientCreditsPost;

        public void CssRpg_ClientCreditsPost(int client, uint iOldCredits, uint iCredits);

        public event Action<int, uint, uint>? ActionClientLevel;

        public void CssRpg_ClientLevel(int client, uint ClientLevel, uint iLevel);

        public event Action<int, uint, uint>? ClientLevelPost;

        public void CssRpg_ClientLevelPost(int client, uint iOldLevel, uint currentLevel);

        public event Action<int, uint, uint>? ActionClientExperience;

        public void CssRpg_ClientExperience(int client, uint ClientExperience, uint iExperience);

        public event Action<int, uint, uint>? ActionClientExperiencePost;

        public void CssRpg_ClientExperiencePost(int client, uint ClientExperiencePost, uint currentLevel);

        public event Action<string>? OnUpgradeRegistered;

        public void CssRpg_OnUpgradeRegistered(string shortName);
    }
}
