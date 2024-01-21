using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WpCShpRpgCoreApi
{
    public interface IWpCShpRpgCoreApi
    {
        public bool IsRpgCoreLoaded();

        public bool UpgradeExists(string ShortName);

        public void RegisterUpgradeType(string sName, string sShortName, string sDescription, uint iMaxLevelBarrier, bool bDefaultEnable,
             uint iDefaultMaxLevel, uint iDefaultStartCost, uint iDefaultCostInc, uint iDefaultAdminFlags, Function queryCallback, Function activeCallback);

        public void UnregisterUpgradeType(string ShortName);

        public Dictionary<string, string> GetParamsFromConfig(string ModuleDirectory, string ShortSkillName);

        public string GetModuleDirectoryImproved();
    }
}
