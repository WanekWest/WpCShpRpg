using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using System.Collections;
using static WpCShpRpg.Upgrades;

namespace WpCShpRpg
{
    public class PlayerData
    {
        private static Config config;
        private static Upgrades upgradesClass;
        private static Database database;
        private static Menu menu;

        public delegate void BuyUpgradeHandler(int client, string shortName, uint currentLevel, ref bool cancel);
        public event BuyUpgradeHandler OnBuyUpgrade;

        public delegate void BuyUpgradePostHandler(int client, string shortName, uint currentLevel);
        public event BuyUpgradePostHandler BuyUpgradePost;

        public delegate void SellUpgradeHandler(int client, string shortName, uint iCurrentLevel, ref bool cancel);
        public event SellUpgradeHandler SellUpgrade;

        public delegate void SellUpgradePostHandler(int client, string shortName, uint currentLevel);
        public event SellUpgradePostHandler SellUpgradePost;

        public delegate void ClientCreditsHandler(int client, uint ClientCredits, uint iCredits, ref bool cancel);
        public event ClientCreditsHandler ClientCredits;

        public delegate void ClientCreditsPostHandler(int client, uint iOldCredits, uint iCredits);
        public event ClientCreditsPostHandler ClientCreditsPost;

        public delegate void ClientLevelHandler(int client, uint ClientLevel, uint iLevel, ref bool cancel);
        public event ClientLevelHandler ClientLevel;

        public delegate void ClientLevelPostHandler(int client, uint iOldLevel, uint currentLevel);
        public event ClientLevelPostHandler ClientLevelPost;

        public delegate void ClientExperienceHandler(int client, uint ClientExperience, uint iExperience, ref bool cancel);
        public event ClientExperienceHandler ClientExperience;

        public delegate void ClientExperiencePostHandler(int client, uint ClientExperiencePost, uint currentLevel);
        public event ClientExperiencePostHandler ClientExperiencePost;

        public PlayerData()
        {

        }

        public void SetDatabase(Database db)
        {
            database = db;
        }

        public void SetUpgrades(Upgrades upgrClass)
        {

            upgradesClass = upgrClass;
        }

        public void SetConfig(Config cfg)
        {
            config = cfg;
        }

        public void SetMenu(Menu mn)
        {
            menu = mn;
        }

        public struct PlayerUpgradeInfo
        {
            public uint purchasedlevel;
            public uint selectedlevel;
            public bool enabled;
            public bool visuals;
            public bool sounds;

            public PlayerUpgradeInfo(uint purchasedlevel, uint selectedlevel, bool enabled, bool visuals, bool sounds)
            {
                this.purchasedlevel = purchasedlevel;
                this.selectedlevel = selectedlevel;
                this.enabled = enabled;
                this.visuals = visuals;
                this.sounds = sounds;
            }
        }

        public struct PlayerInfo
        {
            public uint level;
            public uint experience;
            public uint credits;
            public int dbId;
            public bool showMenuOnLevelup;
            public bool fadeOnLevelup;
            public bool dataLoadedFromDB;
            public ArrayList upgrades;
            public float lastReset;
            public float lastSeen;

            public PlayerInfo(uint level, uint experience, uint credits, int dbId, bool showMenuOnLevelup, bool fadeOnLevelup,
            bool dataLoadedFromDB, ArrayList upgrades, float lastReset, float lastSeen)
            {
                this.level = level;
                this.experience = experience;
                this.credits = credits;
                this.dbId = dbId;
                this.showMenuOnLevelup = showMenuOnLevelup;
                this.fadeOnLevelup = fadeOnLevelup;
                this.dataLoadedFromDB = dataLoadedFromDB;
                this.upgrades = upgrades;
                this.lastReset = lastReset;
                this.lastSeen = lastSeen;
            }
        }

        public struct SessionStats
        {
            public float JoinTime;
            public uint JoinLevel;
            public uint JoinExperience;
            public uint JoinCredits;
            public int JoinRank;
            public bool WantsAutoUpdate;
            public bool WantsMenuOpen;
            public bool OkToClose;
            public List<int> LastExperience; // Используем List<int> вместо ArrayList

            public SessionStats()
            {
                LastExperience = new List<int>();
            }
        }

        public PlayerInfo[] g_iPlayerInfo = new PlayerInfo[Server.MaxPlayers + 1];
        public SessionStats[] g_iPlayerSessionStartStats = new SessionStats[Server.MaxPlayers + 1];
        public bool[] g_bFirstLoaded = new bool[Server.MaxPlayers + 1];
        public string[,] g_sOriginalBotName = new string[Server.MaxPlayers + 1, Player.MaxNameLength];
        public bool[] g_bBackToStatsMenu = new bool[Server.MaxPlayers + 1];

        public void RemovePlayer(int client, bool g_hCVShowMenuOnLevelDefault, bool g_hCVFadeOnLevelDefault, bool bKeepBotName = false)
        {
            ResetStats(client);
            g_iPlayerInfo[client].upgrades.Clear();
            g_iPlayerInfo[client].dbId = -1;
            g_iPlayerInfo[client].dataLoadedFromDB = false;
            g_iPlayerInfo[client].showMenuOnLevelup = g_hCVShowMenuOnLevelDefault;
            g_iPlayerInfo[client].fadeOnLevelup = g_hCVFadeOnLevelDefault;
            g_iPlayerInfo[client].lastReset = 0;
            g_iPlayerInfo[client].lastSeen = 0;

            if (!bKeepBotName)
                g_sOriginalBotName[client, 0] = "\0";
        }

        public bool IsPlayerDataLoaded(int client)
        {
            return g_iPlayerInfo[client].dataLoadedFromDB;
        }

        public float GetPlayerLastReset(int client)
        {
            return g_iPlayerInfo[client].lastReset;
        }

        public void SetPlayerLastReset(int client, float time)
        {
            g_iPlayerInfo[client].lastReset = time;
        }

        public float GetPlayerLastSeen(int client)
        {
            return g_iPlayerInfo[client].lastSeen;
        }

        public PlayerUpgradeInfo GetPlayerUpgradeInfoByIndex(int client, int index)
        {
            return (PlayerUpgradeInfo)g_iPlayerInfo[client].upgrades[index];
        }

        public void SavePlayerUpgradeInfo(int client, int index, PlayerUpgradeInfo playerupgrade)
        {
            g_iPlayerInfo[client].upgrades[index] = playerupgrade;
        }

        public void ResetStats(int client)
        {
            Console.WriteLine("Stats have been reset for player: %N", client);

            int iSize = upgradesClass.GetUpgradeCount();
            Upgrades.InternalUpgradeInfo upgrade;
            PlayerUpgradeInfo playerupgrade;
            bool bWasEnabled;
            for (int i = 0; i < iSize; i++)
            {
                playerupgrade = GetPlayerUpgradeInfoByIndex(client, i);
                // See if this upgrade has been enabled and should be notified to stop the effect.
                bWasEnabled = playerupgrade.enabled && playerupgrade.selectedlevel > 0;

                // Reset upgrade to level 0
                playerupgrade.purchasedlevel = 0;
                playerupgrade.selectedlevel = 0;
                SavePlayerUpgradeInfo(client, i, playerupgrade);

                // No need to inform the upgrade plugin, that this player was reset,
                // if it wasn't active before at all.
                if (!bWasEnabled)
                    continue;

                upgrade = upgradesClass.GetUpgradeByIndex(i);

                if (upgradesClass.IsValidUpgrade(upgrade) == false)
                    continue;
            }

            g_iPlayerInfo[client].level = 1;
            g_iPlayerInfo[client].experience = 0;
            g_iPlayerInfo[client].credits = config.g_hCVCreditsStart;
        }

        public void InitPlayer(int client, bool bGetBotName = true)
        {
            g_bFirstLoaded[client] = true;

            // See if the player should start at a higher level than 1?
            uint[] StartLevelCredits = GetStartLevelAndExperience();

            g_iPlayerInfo[client].level = StartLevelCredits[0];
            g_iPlayerInfo[client].experience = 0;
            g_iPlayerInfo[client].credits = StartLevelCredits[1];
            g_iPlayerInfo[client].dbId = -1;
            g_iPlayerInfo[client].dataLoadedFromDB = false;
            g_iPlayerInfo[client].showMenuOnLevelup = config.g_hCVShowMenuOnLevelDefault;
            g_iPlayerInfo[client].fadeOnLevelup = config.g_hCVFadeOnLevelDefault;
            g_iPlayerInfo[client].lastReset = Server.CurrentTime;
            g_iPlayerInfo[client].lastSeen = Server.CurrentTime;

            g_iPlayerInfo[client].upgrades = new ArrayList();
            int iNumUpgrades = upgradesClass.GetUpgradeCount();

            for (int i = 0; i < iNumUpgrades; i++)
            {
                // start level (default 0) for all upgrades
                InitPlayerNewUpgrade(client);
            }

            // Save the name the bot joined with, so we fetch the right info, even if some plugin changes the name of the bot afterwards.
            if (bGetBotName)
            {
                CCSPlayerController? player = Utilities.GetPlayerFromIndex(client);
                if (player != null && player.IsValid && player.IsBot)
                {
                    g_sOriginalBotName[client, 0] = player.PlayerName;
                }
            }
        }

        public void InsertPlayer(int client, bool g_hCVEnable, bool g_hCVSaveData, bool g_hCVBotSaveStats)
        {
            if (!g_hCVEnable || !g_hCVSaveData || !g_hCVBotSaveStats)
                return;

            string sName;

            CCSPlayerController? player = Utilities.GetPlayerFromIndex(client);
            if (player != null && player.IsValid && !player.IsBot)
            {
                sName = player.PlayerName;
            }
            else
            {
                // TODO: Обработать ошибку.
                return;
            }

            // Make sure to keep the original bot name.
            if (player.IsBot)
            {
                sName = g_sOriginalBotName[client, 0];
            }

            string tableName = "players";
            string query;

            if (!player.IsBot)
            {
                query = $"INSERT INTO {tableName} (name, steamid, level, experience, credits, showmenu, fadescreen, lastseen, lastreset) " +
                        $"VALUES ('{sName}', {player.SteamID}, {GetClientLevel(client)}, {GetClientExperience(client)}, " +
                        $"{GetClientCredits(client)}, {ShowMenuOnLevelUp(client)}, {FadeScreenOnLevelUp(client)}, {Server.CurrentTime}, {Server.CurrentTime})";
            }
            else
            {
                // Для ботов steamid устанавливается как NULL
                query = $"INSERT INTO {tableName} (name, steamid, level, experience, credits, showmenu, fadescreen, lastseen, lastreset) " +
                        $"VALUES ('{sName}', NULL, {GetClientLevel(client)}, {GetClientExperience(client)}, " +
                        $"{GetClientCredits(client)}, {ShowMenuOnLevelUp(client)}, {FadeScreenOnLevelUp(client)}, {Server.CurrentTime}, {Server.CurrentTime})";
            }

            database.SendQuery(query);
        }

        void InitPlayerNewUpgrade(int client)
        {
            // Let the player start this upgrade on its set start level by default.
            ArrayList clienUpgrades = GetClientUpgrades(client);
            int iIndex = clienUpgrades.Count;
            InternalUpgradeInfo upgrade;
            upgrade = upgradesClass.GetUpgradeByIndex(iIndex);

            PlayerUpgradeInfo playerupgrade;
            playerupgrade.purchasedlevel = 0;
            playerupgrade.selectedlevel = 0;
            playerupgrade.enabled = true;
            playerupgrade.visuals = true;
            playerupgrade.sounds = true;
            clienUpgrades.Add(playerupgrade);

            // Get the money for the start level?
            // TODO: Make sure to document the OnBuyUpgrade forward being called on clients not ingame yet + test.
            // (This is can be called OnClientConnected.)
            bool bFree = config.g_hCVUpgradeStartLevelsFree;
            for (int i = 0; i < upgrade.startLevel; i++)
            {
                if (bFree)
                {
                    if (!GiveClientUpgrade(client, iIndex))
                        break;
                }
                else
                {
                    if (!BuyClientUpgrade(client, iIndex))
                        break;
                }
            }
        }

        public bool GiveClientUpgrade(int client, int iUpgradeIndex)
        {
            InternalUpgradeInfo upgrade;
            upgrade = upgradesClass.GetUpgradeByIndex(iUpgradeIndex);

            if (upgradesClass.IsValidUpgrade(upgrade) == false)
                return false;

            uint iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);

            if (iCurrentLevel >= upgrade.maxLevel)
                return false;

            // Upgrade level +1!
            iCurrentLevel++;

            // See if some plugin doesn't want this player to level up this upgrade
            bool cancel = false;
            OnBuyUpgrade?.Invoke(client, upgrade.shortName, iCurrentLevel, ref cancel);

            if (cancel)
                return false;

            // Actually update the upgrade level.
            upgradesClass.SetClientPurchasedUpgradeLevel(client, iUpgradeIndex, iCurrentLevel);
            // Also have it select the new higher upgrade level.
            upgradesClass.SetClientSelectedUpgradeLevel(client, iUpgradeIndex, iCurrentLevel);

            BuyUpgradePost?.Invoke(client, upgrade.shortName, iCurrentLevel);

            return true;
        }

        public bool BuyClientUpgrade(int client, int iUpgradeIndex)
        {
            InternalUpgradeInfo upgrade = upgradesClass.GetUpgradeByIndex(iUpgradeIndex);

            uint iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);

            // can't get higher than this.
            if (iCurrentLevel >= upgrade.maxLevel)
                return false;

            uint iCost = upgradesClass.GetUpgradeCost(iUpgradeIndex, iCurrentLevel + 1);

            // Not enough credits?
            if (iCost > g_iPlayerInfo[client].credits)
                return false;

            if (!GiveClientUpgrade(client, iUpgradeIndex))
                return false;

            g_iPlayerInfo[client].credits -= iCost;

            return true;
        }

        public bool TakeClientUpgrade(int client, int iUpgradeIndex)
        {
            InternalUpgradeInfo upgrade = upgradesClass.GetUpgradeByIndex(iUpgradeIndex);

            if (upgradesClass.IsValidUpgrade(upgrade) == false)
                return false;

            uint iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);

            // Can't get negative levels
            if (iCurrentLevel <= 0)
                return false;

            // Upgrade level -1!
            iCurrentLevel--;

            bool cancel = false;
            SellUpgrade?.Invoke(client, upgrade.shortName, iCurrentLevel, ref cancel);

            if (cancel)
                return false;

            // Actually update the upgrade level.
            upgradesClass.SetClientPurchasedUpgradeLevel(client, iUpgradeIndex, iCurrentLevel);

            SellUpgradePost?.Invoke(client, upgrade.shortName, iCurrentLevel);

            return true;
        }

        public bool SellClientUpgrade(int client, int iUpgradeIndex)
        {
            InternalUpgradeInfo upgrade = upgradesClass.GetUpgradeByIndex(iUpgradeIndex);

            uint iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);

            if (iCurrentLevel <= 0)
                return false;

            if (!TakeClientUpgrade(client, iUpgradeIndex))
                return false;

            g_iPlayerInfo[client].credits += upgradesClass.GetUpgradeSale(iUpgradeIndex, iCurrentLevel);

            return true;
        }

        // Have bots buy upgrades too :)
        void BotPickUpgrade(int client)
        {
            bool upgradeBought;
            int currentIndex;

            int size = upgradesClass.GetUpgradeCount();

            ArrayList randomBuying = new ArrayList();
            for (int i = 0; i < size; i++)
                randomBuying.Add(i);

            while (GetClientCredits(client) > 0)
            {
                // Shuffle the order of upgrades randomly
                var random = new Random();
                randomBuying = new ArrayList(randomBuying.Cast<object>().OrderBy(x => random.Next()).ToList());

                upgradeBought = false;
                for (int i = 0; i < size; i++)
                {
                    currentIndex = (int)randomBuying[i];
                    var upgrade = upgradesClass.GetUpgradeByIndex(currentIndex);

                    // Valid upgrade the bot can use?
                    if (!upgradesClass.IsValidUpgrade(upgrade) || !upgrade.enabled)
                        continue;

                    // Don't buy it, if bots aren't allowed to use it at all.
                    if (!upgrade.allowBots)
                        continue;

                    // Don't let him buy upgrades, which are restricted to the other team.
                    if (upgradesClass.IsClientInLockedTeam(client, upgrade) == false)
                        continue;

                    if (BuyClientUpgrade(client, currentIndex))
                    {
                        upgradeBought = true;
                        break;
                    }
                }
                if (!upgradeBought)
                    break; // Couldn't afford anything
            }
        }

        /**
         * Player info accessing functions (getter/setter)
         */
        public void CheckItemMaxLevels(int client)
        {
            int iSize = upgradesClass.GetUpgradeCount();
            InternalUpgradeInfo upgrade;
            uint iMaxLevel;
            uint iCurrentLevel;
            for (int i = 0; i < iSize; i++)
            {
                upgrade = upgradesClass.GetUpgradeByIndex(i);
                iMaxLevel = upgrade.maxLevel;
                iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, i);
                while (iCurrentLevel > iMaxLevel)
                {
                    /* Give player their credits back */
                    SetClientCredits(client, GetClientCredits(client) + upgradesClass.GetUpgradeCost(i, iCurrentLevel--));
                }
                if (upgradesClass.GetClientPurchasedUpgradeLevel(client, i) != iCurrentLevel)
                    upgradesClass.SetClientPurchasedUpgradeLevel(client, i, iCurrentLevel);
            }
        }

        public uint GetClientCredits(int client)
        {
            return g_iPlayerInfo[client].credits;
        }

        bool SetClientCredits(int client, uint iCredits)
        {
            if (iCredits < 0)
                iCredits = 0;

            bool cancel = false;
            ClientCredits?.Invoke(client, g_iPlayerInfo[client].credits, iCredits, ref cancel);

            if (cancel)
                return false;

            uint iOldCredits = g_iPlayerInfo[client].credits;
            g_iPlayerInfo[client].credits = iCredits;

            ClientCreditsPost?.Invoke(client, iOldCredits, iCredits);

            return true;
        }

        public uint GetClientLevel(int client)
        {
            return g_iPlayerInfo[client].level;
        }

        public bool SetClientLevel(int client, uint iLevel)
        {
            if (iLevel < 1)
                iLevel = 1;

            bool cancel = false;
            ClientLevel?.Invoke(client, g_iPlayerInfo[client].level, iLevel, ref cancel);

            if (cancel)
                return false;

            uint iOldLevel = g_iPlayerInfo[client].level;
            g_iPlayerInfo[client].level = iLevel;

            ClientLevelPost?.Invoke(client, iOldLevel, iLevel);

            return true;
        }

        public uint GetClientExperience(int client)
        {
            return g_iPlayerInfo[client].experience;
        }

        public bool SetClientExperience(int client, uint iExperience)
        {
            if (iExperience < 0)
                iExperience = 0;

            bool cancel = false;
            ClientExperience?.Invoke(client, g_iPlayerInfo[client].experience, iExperience, ref cancel);

            if (cancel)
                return false;

            uint iOldExperience = g_iPlayerInfo[client].experience;
            g_iPlayerInfo[client].experience = iExperience;

            ClientExperiencePost?.Invoke(client, iOldExperience, iExperience);

            return true;
        }

        public uint[] GetStartLevelAndExperience()
        {
            // See if the player should start at a higher level than 1?
            uint[] StartLevelCredits = { config.g_hCVLevelStart, config.g_hCVCreditsStart };
            if (StartLevelCredits[0] < 1)
                StartLevelCredits[0] = 1;

            // If the start level is at a higher level than 1, he might get more credits for his level.
            if (config.g_hCVLevelStartGiveCredits)
                StartLevelCredits[1] += config.g_hCVCreditsInc * (StartLevelCredits[0] - 1);

            return StartLevelCredits;
        }

        public ArrayList GetClientUpgrades(int client)
        {
            return g_iPlayerInfo[client].upgrades;
        }

        public uint GetClientSelectedUpgradeLevel(int client, int iUpgradeIndex)
        {
            PlayerUpgradeInfo playerupgrade = GetPlayerUpgradeInfoByIndex(client, iUpgradeIndex);
            return playerupgrade.selectedlevel;
        }

        public uint GetClientPurchasedUpgradeLevel(int client, int iUpgradeIndex)
        {
            PlayerUpgradeInfo playerupgrade = GetPlayerUpgradeInfoByIndex(client, iUpgradeIndex);
            return playerupgrade.purchasedlevel;
        }

        bool ShowMenuOnLevelUp(int client)
        {
            return g_iPlayerInfo[client].showMenuOnLevelup;
        }

        void SetShowMenuOnLevelUp(int client, bool show)
        {
            g_iPlayerInfo[client].showMenuOnLevelup = show;
        }

        bool FadeScreenOnLevelUp(int client)
        {
            return g_iPlayerInfo[client].fadeOnLevelup;
        }

        void SetFadeScreenOnLevelUp(int client, bool fade)
        {
            g_iPlayerInfo[client].fadeOnLevelup = fade;
        }


        public void InitPlayerSessionStartStats(int client)
        {
            g_iPlayerSessionStartStats[client] = new SessionStats
            {
                JoinTime = Server.CurrentTime, // предполагается, что это значение времени в вашей системе
                JoinLevel = GetClientLevel(client), // аналогично вызову функции в вашей системе
                JoinExperience = GetClientExperience(client),
                JoinCredits = GetClientCredits(client),
                JoinRank = -1,
                WantsAutoUpdate = false,
                WantsMenuOpen = false,
                OkToClose = false,
                LastExperience = new List<int>(config.g_hCVLastExperienceCount) // размер списка инициализируется в соответствии с требуемым количеством элементов
            };
        }

        public void ResetPlayerSessionStats(int client)
        {
            g_iPlayerSessionStartStats[client].JoinTime = 0.0f;
            g_iPlayerSessionStartStats[client].JoinLevel = 0;
            g_iPlayerSessionStartStats[client].JoinExperience = 0;
            g_iPlayerSessionStartStats[client].JoinCredits = 0;
            g_iPlayerSessionStartStats[client].JoinRank = -1;
            g_iPlayerSessionStartStats[client].WantsAutoUpdate = false;
            g_iPlayerSessionStartStats[client].WantsMenuOpen = false;
            g_iPlayerSessionStartStats[client].OkToClose = false;
            g_iPlayerSessionStartStats[client].LastExperience.Clear();
        }
    }
}