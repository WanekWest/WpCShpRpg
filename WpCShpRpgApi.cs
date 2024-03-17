using CounterStrikeSharp.API;
using WpCShpRpg.Core.Additions;
using WpCShpRpgCoreApi;
using static WpCShpRpgCoreApi.IWpCShpRpgCoreApi;

namespace WpCShpRpg.Core
{
    public class WpCShpRpgApi : IWpCShpRpgCoreApi
    {
        private readonly WpCShpRpg _WpCShpRpgCore;
        public string WpCShpRpg { get; }

        private ConfiguraionFiles config;
        private string ModuleDirectoryImproved;

        public WpCShpRpgApi(WpCShpRpg helloWorldCore)
        {
            _WpCShpRpgCore = helloWorldCore; // Сохранение полученного экземпляра ядра в закрытое поле.
        }

        public WpCShpRpgApi(ref ConfiguraionFiles cfg, string moduleDirectoryImproved)
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
             uint iDefaultMaxLevel, uint iDefaultStartCost, uint iDefaultCostInc)
        {
            Upgrades.RegisterUpgradeType(sName, sShortName, sDescription, iMaxLevelBarrier, bDefaultEnable,
             iDefaultMaxLevel, iDefaultStartCost, iDefaultCostInc);

            return;
        }

        public void UnregisterUpgradeType(string ShortName)
        {
            Upgrades.UnregisterUpgradeType(ShortName);

            return;
        }


        public event Action<string, TranslationType>? TranslateUpgradeCallBack;

        public void CssRpg_TranslateUpgrade(string shortname, TranslationType type)
        {
            InternalUpgradeInfo upgrade = new();
            if (!GetUpgradeByShortname(shortname, ref upgrade) || !Upgrades.IsValidUpgrade(upgrade))
            {
                Server.PrintToConsole($"No upgrade named {shortname} loaded.");
                return;
            }

            if (TranslateUpgradeCallBack != null)
                TranslateUpgradeCallBack?.Invoke(shortname, type);

            Upgrades.SaveUpgradeConfig(upgrade);
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

        public uint GetClientUpgradeLevel(int client, string shortname)
        {
            return PlayerData.GetClientUpgradeLevel(client, shortname);
        }

        public bool GetUpgradeByShortname(string sShortName, ref InternalUpgradeInfo upgrade)
        {
            return Upgrades.GetUpgradeByShortname(sShortName, ref upgrade);
        }

        public event Action<int, IWpCShpRpgCoreApi.UpgradeQueryType, string>? CssRpg_UpgradeBuySell;

        public void CssRpg_BuySell(int client, IWpCShpRpgCoreApi.UpgradeQueryType queryType, string UpgradeShortName)
        {
            if (CssRpg_UpgradeBuySell != null)
            {
                CssRpg_UpgradeBuySell?.Invoke(client, queryType, UpgradeShortName);
            }
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
