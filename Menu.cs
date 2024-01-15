namespace WpCShpRpg
{
    public class Menu
    {
        private static Database database = null;
        private static Config config = null;
        private static PlayerData playerData = null;
        private static Upgrades upgrades = null;

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
    }
}
